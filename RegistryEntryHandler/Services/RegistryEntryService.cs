using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using RegistryEntryService;
using SteelCloud.Windows.PowerShellEngine.JobConfiguration;
using SteelCloud.Windows.PowerShellEngine.Utilities;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using SteelCloud.Windows.PowerShellEngine.Common;
using SteelCloud.CommonEnumerations;
using DatabaseService;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace RegistryEntryHandler.Services
{
    
    public class RegistryEntryService : RegistryEntry.RegistryEntryBase
    {
        public virtual Runspace PowerShellRunSpace { get; set; }
        public List<PowerShellCommand> PowerShellCommandsToRun { get; set; }
        //public Runspace SessionRunspace { get; }
        public override async Task ScanRegistryEntry(RegistryEntryRequest request, IServerStreamWriter<RegistryEntryResponse> responseStream, ServerCallContext context)
        {
            //Console.WriteLine($"'ScanRegistryEntry' was invoked with: {request}");

            var preparingRunspace = PrepareRunspace(request.UserName, request.UserPassword, request.EndpointAddress);
            if (string.IsNullOrEmpty(preparingRunspace))
            {
                int passed = 0;
                int failed = 0;
                PowerShellCommandsToRun = new List<PowerShellCommand>();
                foreach (var registryEntry in request.Children)
                {
                    foreach(var registryEntryItem in registryEntry.MRegistryEntryItems)
                    {
                        Console.WriteLine($"Scanning {registryEntryItem.RegistryValueName}");
                        var command = GenerateRegistryEntryPowerShellCommand(registryEntryItem, enmProcesType.SCAN);
                        command.RunCommand(PowerShellRunSpace);
                        var currentValue = GetCurrentValue(command);
                        var complianceValue = registryEntryItem.MIControlPartType.UseCustomComplianceValue ? registryEntryItem.MIControlPartType.CustomComplianceValue : registryEntryItem.MIControlPartType.BaselineComplianceValue;
                        var passFailResult = GetPassFailResult(currentValue, complianceValue);
                        if (passFailResult)
                        {
                            passed++;
                        }
                        else
                        {
                            failed++;
                        }

                        var registryEntryResponse = new RegistryEntryResponse()
                        {
                            Response = $"{registryEntryItem.RegistryValueName} successfully scanned."
                        };

                        await responseStream.WriteAsync(registryEntryResponse);
                        //System.Threading.Thread.Sleep(200);
                    }
                }
            }
            
        }

        private string PrepareRunspace(string p_userName, string p_userPassword, string endpointAddress)
        {
            var domainUserName = IpUtilities.GetDomainAndUserFromUserName(p_userName);
            var job = new JobConfiguration(endpointAddress, new Credentials($@"{domainUserName.Item1}\{domainUserName.Item2}", Credentials.ConvertToSecureString(p_userPassword)));

            PowerShellRunSpace = PowerShellUtilities.GetPowerShellRunSpace(job, endpointAddress);
            if (PowerShellRunSpace != null)
            {
                try
                {
                    PowerShellUtilities.SetUpRemotePowershellConnection(job);
                    try
                    {
                        PowerShellRunSpace.Open();
                    }
                    catch (Exception exc)
                    {
                        return $"Unable to open PowerShell runspace. {exc.Message}";
                    }
                }
                catch (Exception exc)
                {
                    return $"Unable to connect to PowerShell. {exc.Message}";
                }
            }
            return string.Empty;
        }

        public static PowerShellCommand GenerateRegistryEntryPowerShellCommand(MRegistryEntryItem p_registryEntry, enmProcesType p_processType)
        {
            var script = p_processType == enmProcesType.SCAN ? GenerateScanScript(p_registryEntry) : GenerateRemediateScript(p_registryEntry);
            return new PowerShellCommand(script, p_registryEntry);
        }

        private static string GenerateScanScript(MRegistryEntryItem p_registryEntry)
        {
            var registryPath = $@"{RegistryHivesDictionary.FirstOrDefault(p_o => p_o.Value == p_registryEntry.RegistryKeyRoot).Key}:\{p_registryEntry.RegistrySubKey}";
            return $@"Get-ItemProperty -Path ""{registryPath}"" | Select-Object -ExpandProperty {p_registryEntry.RegistryValueName}";
        }

        private static string GenerateRemediateScript(MRegistryEntryItem p_registryEntry)
        {
            var registryPath = $@"{RegistryHivesDictionary.FirstOrDefault(p_o => p_o.Value == p_registryEntry.RegistryKeyRoot).Key}:\{p_registryEntry.RegistrySubKey}";
            var registryKind = $@"{RegistryKindDictionary.FirstOrDefault(p_o => p_o.Value == p_registryEntry.RegistryValueKind).Key}";
            var valueToSet = p_registryEntry.MIControlPartType.BaselineComplianceValue;
            //if (!string.IsNullOrEmpty(p_registryEntry.DefaultRemediationValue))
            //{
            //    //valueToSet = p_registryEntry.DefaultRemediationValue;
            //}
            if (p_registryEntry.RegistryValueKind == "REG_MULTI_SZ")
            {
                //valueToSet = "'" + valueToSet.Replace(",", "','") + "'";
            }
            return $@"New-Item -Path ""{registryPath}"" -Force; New-ItemProperty -Path ""{registryPath}"" -Name {p_registryEntry.RegistryValueName} -Value {valueToSet} -PropertyType {registryKind} -Force";
        }

        //TODO move this two methods to ConfigOS common libraries
        public static readonly Dictionary<string, string> RegistryHivesDictionary = new Dictionary<string, string>()
        {
            {"HKLM", "HKEY_LOCAL_MACHINE" },
            {"HKCU", "HKEY_CURRENT_USER"},
            {"HKCR", "HKEY_CLASSES_ROOT" },
            {"HKCC", "HKEY_CURRENT_CONFIG" },
            {"HKPD", "HKEY_PERFORMANCE_DATA" },
            {"HKU", "HKEY_USERS" }
        };

        public static readonly Dictionary<string, string> RegistryKindDictionary = new Dictionary<string, string>()
        {
            {"BINARY", "REG_BINARY" },
            {"DWORD", "REG_DWORD" },
            {"STRING", "REG_SZ"},
            {"EXPANDSTRING", "REG_EXPAND_SZ" },
            {"MULTISTRING", "REG_MULTI_SZ" }
        };

        private string GetCurrentValue(PowerShellCommand p_commandResult)
        {
            var output = string.Empty;
            if (p_commandResult.CommandOutput.Any())
            {
                foreach (var commandOutput in p_commandResult.CommandOutput.ToList())
                {
                    output = output + commandOutput + ",";
                }
                output = output.TrimEnd(',');
            }
            return output;
        }

        private bool GetPassFailResult(string _source, string _dest)
        {
            var SysVal = "";
            var isRXLogic = false;
            var RXLMatch = false;
            var RXLogicType = "";
            var RXLogicsValue = "";
            _dest = _dest.ToUpper().Replace("VALUE:", "").Replace(" ", "");
            _source = _source.ToUpper().Replace("VALUE:", "").Replace(" ", "");

            //Detect Lesser or Equal
            if (Regex.IsMatch(_dest, @"^\[MAX\.\.\.\d+\],", RegexOptions.IgnoreCase))
            {
                isRXLogic = true;
                RXLogicType = "__LESSEROREQUAL";
            }
            //Detect Greater or Equal
            else if (Regex.IsMatch(_dest, @"^\[MIN\.\.\.\d+\],", RegexOptions.IgnoreCase))
            {
                isRXLogic = true;
                RXLogicType = "__GREATEROREQUAL";
            }
            //Detect Range.....
            else if (Regex.IsMatch(_dest, @"^\[\d+\.\.\.\d+\],", RegexOptions.IgnoreCase))
            {
                isRXLogic = true;
                RXLogicType = "__RANGE";
            }
            //Detect Neither Or
            else if (Regex.IsMatch(_dest, @"^\[\w+", RegexOptions.IgnoreCase)
                     && Regex.IsMatch(_dest, @"((?:\w+)(?:\|))+\w+", RegexOptions.IgnoreCase)
                     && Regex.IsMatch(_dest, @"\w+\],", RegexOptions.IgnoreCase))
            {
                isRXLogic = true;
                RXLogicType = "__EITHEROR";
            }
            else if (Regex.IsMatch(_dest, @"^\[.*\|.*\]", RegexOptions.IgnoreCase))
            {
                isRXLogic = true;
                RXLogicType = "__EQUALVALUEORNULL";
            }
            //Detect Not Equal
            else if (Regex.IsMatch(_dest, @"^\[!\b\w+\b\],", RegexOptions.IgnoreCase)
                     || Regex.IsMatch(_dest, @"^\[!""\s*""],", RegexOptions.IgnoreCase)
                     || Regex.IsMatch(_dest, @"^\[!""(\s*\b\w+\b\s*)+""],", RegexOptions.IgnoreCase))
            {
                isRXLogic = true;
                RXLogicType = "__NOTEQUAL";
            }
            else if (Regex.Replace(_dest, @"\s+", "").StartsWith("[\"\"],"))
            {
                isRXLogic = true;
                RXLogicType = "__EQUALNULLOREMPTY";
            }
            else if (Regex.Replace(_dest, @"\s+", "").StartsWith("[")
                     && Regex.IsMatch(_dest, @"^\[""[^\s]+""\],")) //match nonspace between double quotes
            {
                isRXLogic = true;
                RXLogicType = "__EXPLICITOTHER";
            }
            else if (Regex.Replace(_dest, @"\s+", "").StartsWith("[")
                     && Regex.IsMatch(_dest, @"^\[[^\s].*\],"))
            {
                isRXLogic = true;
                RXLogicType = "__REGEXSTR";
            }
            //Get system Value and RXlogics Value
            if (!isRXLogic) return (RXLMatch);

            var RXDArr = RXLogicType == "__EQUALVALUEORNULL" ? _dest.Replace("[", "").Replace("]", "").Split('|') : _dest.Split(',');
            SysVal = _source;

            switch (RXLogicType)
            {
                case "__RANGE":
                    if (RXDArr.Length > 1)
                    {
                        RXLogicsValue = RXDArr[0].Replace("..", "").Replace("[", "").Replace("]", "");
                        var N1 = RXLogicsValue.Split('.')[0];
                        var N2 = RXLogicsValue.Split('.')[1];
                        if (!string.IsNullOrEmpty(N1) && !string.IsNullOrEmpty(N2))
                        {
                            try
                            {
                                var NumN1 = Convert.ToInt32(N1.Replace("\"", ""));
                                var NumN2 = Convert.ToInt32(N2.Replace("\"", ""));
                                var NumSysVal = Convert.ToInt32(SysVal.Replace("\"", ""));
                                if (NumSysVal >= NumN1 && NumSysVal <= NumN2)
                                {
                                    RXLMatch = true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            catch
                            {
                                RXLMatch = false;
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }
                    break;
                case "__GREATEROREQUAL":
                    if (RXDArr.Length > 0)
                    {
                        RXLogicsValue = RXDArr[0].Replace("MIN...", "").Replace("[", "").Replace("]", "");
                        var N1 = RXLogicsValue;
                        if (!string.IsNullOrEmpty(N1))
                        {
                            try
                            {
                                var NumN1 = Convert.ToInt32(N1.Replace("\"", ""));
                                var NumSysVal = Convert.ToInt32(SysVal.Replace("\"", ""));
                                RXLMatch = NumSysVal >= NumN1;
                            }
                            catch
                            {
                                RXLMatch = false;
                            }
                        }
                    }
                    break;
                case "__LESSEROREQUAL":
                    if (RXDArr.Length > 0)
                    {
                        RXLogicsValue = RXDArr[0].Replace("MAX...", "").Replace("[", "").Replace("]", "");
                        var N1 = RXLogicsValue;
                        if (!string.IsNullOrEmpty(N1))
                        {
                            try
                            {
                                var NumN1 = Convert.ToInt32(N1.Replace("\"", ""));
                                var NumSysVal = Convert.ToInt32(SysVal.Replace("\"", ""));
                                RXLMatch = NumSysVal <= NumN1;
                            }
                            catch
                            {
                                RXLMatch = false;
                            }
                        }
                    }
                    break;
                case "__EITHEROR":
                    if (RXDArr.Length > 1)
                    {
                        RXLogicsValue = RXDArr[0].Replace("[", "").Replace("]", "");
                        var RXLogicsValARR = RXLogicsValue.Split('|');
                        foreach (var elementSTR in RXLogicsValARR)
                        {
                            if (string.IsNullOrEmpty(elementSTR)) continue;

                            if (SysVal == elementSTR)
                            {
                                RXLMatch = true;
                                break;
                            }
                            if (SysVal == elementSTR.Replace("\"", ""))
                            {
                                RXLMatch = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        RXLMatch = false;
                    }
                    break;
                case "__EQUALVALUEORNULL":
                    if (RXDArr.Length > 0)
                    {
                        foreach (var elementSTR in RXDArr)
                        {
                            if (string.IsNullOrEmpty(elementSTR)) continue;

                            if (SysVal == elementSTR)
                            {
                                RXLMatch = true;
                                break;
                            }
                            if (SysVal == elementSTR.Replace("\"", ""))
                            {
                                RXLMatch = true;
                                break;
                            }
                        }
                    }
                    break;
                case "__NOTEQUAL":
                    if (RXDArr.Length > 0)
                    {
                        RXLogicsValue = RXDArr[0].Replace("!", "").Replace("[", "").Replace("]", "");
                        var N1 = RXLogicsValue;

                        if (!string.IsNullOrEmpty(N1))
                        {
                            if (N1 == "\"\"")
                            {
                                N1 = "";
                            }
                            if (SysVal.ToUpper() != N1)
                            {
                                RXLMatch = true;
                            }
                        }
                    }
                    break;
                case "__EQUALNULLOREMPTY":
                    RXLMatch = string.IsNullOrEmpty(SysVal);
                    break;
                case "__EXPLICITOTHER":
                    if (RXDArr.Length > 0)
                    {
                        RXLogicsValue = Regex.Replace(RXDArr[0], @"[\[""\]]", "");
                        RXLMatch = SysVal == RXLogicsValue;
                    }
                    break;
                case "__REGEXSTR":
                    if (RXDArr.Length > 0)
                    {
                        var __inverseFinding = false;
                        RXLogicsValue = _dest.Split(',')[0].TrimStart('[').TrimEnd(']');

                        if (RXLogicsValue[0] == '!')
                        {
                            __inverseFinding = true;
                            RXLogicsValue = RXLogicsValue.TrimStart('!');
                        }

                        if (verifyRegEx(RXLogicsValue))
                        {
                            var _m = Regex.Match(_source, RXLogicsValue);
                            if (_m.Success)
                            {
                                if (!__inverseFinding)
                                {
                                    RXLMatch = true;
                                }
                            }
                            else
                            {
                                if (__inverseFinding)
                                {
                                    RXLMatch = true;
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            return RXLMatch;
        }
        private bool verifyRegEx(string _regexSTR)
        {
            var isValid = true;

            if (_regexSTR != null && _regexSTR.Trim().Length > 0)
            {
                try
                {
                    Regex.Match("", _regexSTR);
                }
                catch (ArgumentException)
                {
                    isValid = false;
                }
            }
            else
            {
                isValid = false;
            }

            return isValid;
        }
    }

    public class PowerShellCommand
    {
        public PowerShellCommand(string p_script, MRegistryEntryItem p_controlPart)
        {
            Script = p_script;

            ControlPart = p_controlPart;

            CommandOutput = new List<PSObject>();

            ErrorOutput = string.Empty;

        }

        public MRegistryEntryItem ControlPart { get; set; }
        public string Script { get; set; }
        public IEnumerable<PSObject> CommandOutput { get; set; }
        public string ErrorOutput { get; set; }
        public Runspace Runspace { get; set; }

        public bool HasErrors()
        {
            return !string.IsNullOrWhiteSpace(ErrorOutput);
        }

        public void RunCommand(Runspace p_runspace)
        {
            using (var ps = PowerShell.Create())
            {
                if (p_runspace != null)
                {
                    ps.Runspace = p_runspace;
                }

                ps.AddScript(Script);

                try
                {
                    CommandOutput = ps.Invoke();
                }
                catch (Exception e)
                {
                    Trace.TraceError("Error occurred in PowerShell script: " + e);
                    ErrorOutput = e.Message;
                }

                if (ps.Streams.Error.Count > 0)
                {
                    ErrorOutput = string.Join(Environment.NewLine, ps.Streams.Error.Select(e => e.ToString()));
                }
            }
        }
    }
}
