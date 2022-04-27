using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Delimon.Win32.IO;
//using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;

namespace MsVSBuildConfigurator
{
    /*public static class CloneExtension
    {
        public static T Clone<T>(this T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            var stream = new System.IO.MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, System.IO.SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
    }*/
    public static class LinqExtension
    {
        public static T PrioritizedFind<T>(this IEnumerable<T> source, params Func<T, bool>[] predicates) where T : class
        {
            foreach (var predicate in predicates)
            {
                var finded = source.FirstOrDefault(predicate);
                if (finded != default(T))
                    return finded;
            }
            return default(T);
        }

        public static HashSet<TKey> ToHashSet<TKey>(
            this IEnumerable<TKey> source,
            IEqualityComparer<TKey> comparer = null)
        {
            return new HashSet<TKey>(source, comparer);
        }

        public static string ReplaceIfNotNullOrWhiteSpace(this string source, string from, string to)
        {
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to) || string.IsNullOrWhiteSpace(source))
                return source;
            return source.Replace(from, to);
        }
    }

    public class RegexUpdater
    {
        private readonly string _updatedConfigPath;
        private List<Tuple<Regex, string>> _regexUpdaters;

        public RegexUpdater(string updatedConfigPath)
        {
            _updatedConfigPath = updatedConfigPath;
            _regexUpdaters = new List<Tuple<Regex, string>>();
        }

        public string Patched
        {
            get { return _updatedConfigPath; }
        }

        public RegexUpdater ReplaceOrAdd(string updatedRegEx, string toStringValue)
        {
            var updating = Tuple.Create(new Regex(updatedRegEx, RegexOptions.Compiled | RegexOptions.IgnoreCase), toStringValue);
            _regexUpdaters.Add(updating);
            return this;
        }

        public RegexUpdater Apply()
        {
            string source = File.ReadAllText(_updatedConfigPath);
            foreach (var updater in _regexUpdaters)
                ReplaceOrAdd(updater.Item1, ref source, updater.Item2);
            File.WriteAllText(_updatedConfigPath, source);
            return this;
        }

        private static void ReplaceOrAdd(Regex regex, ref string source, string value)
        {
            if (regex.IsMatch(source))
                source = regex.Replace(source, value);
            else
                source += Environment.NewLine + value;
        }

        public RegexUpdater AppendSinceTo(IEnumerable<string> sinceAnyOf, string to, string[] values)
        {
            string source = File.ReadAllText(_updatedConfigPath);
            int begin = -1;
            foreach (var since in sinceAnyOf)
            {
                begin = source.IndexOf(since);
                if (begin == -1)
                    continue;
                begin = begin + since.Count();
                break;
            }
            if (begin == -1)
                throw new InvalidOperationException($"Can't update {_updatedConfigPath} because of not found {nameof(sinceAnyOf)}: '{sinceAnyOf.FirstOrDefault()}' or others");
            var end = source.IndexOf(to, begin);
            if (end == -1)
                throw new InvalidOperationException($"Can't update {_updatedConfigPath} because of not found {nameof(to)}: '{to}'");
            var destination = new StringBuilder();
            var pro = source.Substring(0, begin);
            destination.Append(pro);
            var old = source.Substring(begin, end - begin);


            destination.Append(old);
            foreach (var value in values.OrderBy(_ => _))
            {
                if (!old.Contains(value.Trim()))
                    destination.AppendLine(value);
            }


            var epil = source.Substring(end);
            destination.Append(epil);
            var result = destination.ToString();
            File.WriteAllText(_updatedConfigPath, result);
            return this;
        }

        private List<(string Guid, string Configuration, string Platform, string ConfigType, string Configuration2, string Platform2, string LineEnding)> ProjectConfigurationsExtracter(string source)
        {
            var extracted = Program.RegexGroupMatchList(source, Program.ProjectConfigurationRegex.Value, null, 1, 2, 3, 4 ,5, 6, 7);
            return extracted.Select(projConf => (projConf[0], projConf[1], projConf[2], projConf[3], projConf[4], projConf[5], projConf[6])).ToList();
        }

        private string ProjectConfigurationCreater((string Guid, string Configuration, string Platform, string ConfigType, string Configuration2, string Platform2, string LineEnding) projConf)
        {
            return $"\t\t{projConf.Guid}.{projConf.Configuration}|{projConf.Platform}.{projConf.ConfigType} = {projConf.Configuration2}|{projConf.Platform2}{projConf.LineEnding}";
        }

        public RegexUpdater UpdateProjectsConfigurationSinceTo(IEnumerable<string> sinceAnyOf, string to)
        {
            string source = File.ReadAllText(_updatedConfigPath);
            int begin = -1;
            foreach (var since in sinceAnyOf)
            {
                begin = source.IndexOf(since);
                if (begin == -1)
                    continue;
                begin = begin + since.Count();
                break;
            }
            if (begin == -1)
                throw new InvalidOperationException($"Can't update {_updatedConfigPath} because of not found {nameof(sinceAnyOf)}: '{sinceAnyOf.FirstOrDefault()}' or others");
            var end = source.IndexOf(to, begin);
            if (end == -1)
                throw new InvalidOperationException($"Can't update {_updatedConfigPath} because of not found {nameof(to)}: '{to}'");
            var destination = new StringBuilder();
            var pro = source.Substring(0, begin);
            destination.Append(pro);
            var old = source.Substring(begin, end - begin);

            var projConfs = ProjectConfigurationsExtracter(old);
            projConfs = projConfs.SelectMany(_ => new[] 
            {
                _,
                (_.Guid
                    , Program.ConfigurationNameMutator(new ConfigurationPlatform() { Configuration = _.Configuration, Platform = _.Platform })
                    , _.Platform
                    , _.ConfigType
                    , Program.ConfigurationNameMutator(new ConfigurationPlatform() { Configuration = _.Configuration2, Platform = _.Platform })
                    , _.Platform2
                    , _.LineEnding)
            }).Distinct().ToList();
            projConfs = projConfs
                .OrderBy(_ => _.Guid)
                .ThenBy(_ => _.Configuration)
                .ThenBy(_ => _.Platform)
                .ThenBy(_ => _.ConfigType)
                .ToList();
            var new_ = string.Join("", projConfs
                .Select(ProjectConfigurationCreater));
            destination.Append(new_);
            var epil = source.Substring(end);
            destination.Append(epil);
            var result = destination.ToString();
            File.WriteAllText(_updatedConfigPath, result);
            return this;
        }
    }

    public enum NodeType
    {
        ProjectConfiguration = 1
        , PropertyGroup_Configuration
        , PropertyGroup_LabelIsNotConfiguration
        , PropertyGroup_Unclassified_subitems
        , PropertyGroup_IntDir_subitem
        , PropertyGroup_OutDir_subitem
        , PropertyGroup_TargetName_subitem
        , ItemDefinitionGroup
        , ImportGroup_LabelIsAny
        , ItemGroup_ClCompile_AdditionalOptions_subitem
        , ItemGroup_ClCompile_PreprocessorDefinitions_subitem
        , ItemGroup_ClCompile_subitem
        , ItemGroup_Link_subitem
        , ItemGroup_Unclassified_subitem
    }

    public class TypeToNode
    {
        public NodeType Type { get; set; }
        public XElement Node { get; set; }
    }

    public class ConfigurationPlatform : IEquatable<ConfigurationPlatform>
    {
        public string Configuration { get; set; }
        public string Platform { get; set; }

        public ConfigurationPlatform()
        {
            Configuration = null;
            Platform = null;
        }

        public ConfigurationPlatform(ConfigurationPlatform source)
        {
            Configuration = source.Configuration/*.Clone()*/;
            Platform = source.Platform/*.Clone()*/;
        }

        public string ToMinimalMainCondition()
        {
            return $"{Configuration}|{Platform}";
        }

        public string ToMainCondition()
        {
            return $"'$(Configuration)|$(Platform)'=='{ToMinimalMainCondition()}'";
        }

        public string ToMainConditionQuoted()
        {
            return @"""" + ToMainCondition() + @"""";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ConfigurationPlatform);
        }

        public bool Equals(ConfigurationPlatform other)
        {
            return other != null &&
                   Configuration == other.Configuration &&
                   Platform == other.Platform;
        }

        public override int GetHashCode()
        {
            var hashCode = 1703153171;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Configuration);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Platform);
            return hashCode;
        }

        public static bool operator ==(ConfigurationPlatform platform1, ConfigurationPlatform platform2)
        {
            return EqualityComparer<ConfigurationPlatform>.Default.Equals(platform1, platform2);
        }

        public static bool operator !=(ConfigurationPlatform platform1, ConfigurationPlatform platform2)
        {
            return !(platform1 == platform2);
        }
    }

    //[Serializable]
    public class SolutionInfo
    {
        public static HashSet<string> UpdatedMainConfigurationProjectFullFilePath { get; } = new HashSet<string>();
        public string SolutionFilePath { get; set; }
        public string ProjectFilePath { get; set; }
        public string ProjectFullFilePath { get; set; }
        public string SolutionError { get; set; }
        public bool RootSolution{ get; set; }
        public bool ConfigurationUpdateAllowed { get; set; }
        public bool ConfigurationUpdateNeeded { get { return ConfigurationUpdateAllowed; } }
        public HashSet<string> ProjectOutputs { get; } = new HashSet<string>();
        public HashSet<string> ProjectInputs { get; } = new HashSet<string>();
        public IList<ConfigurationPlatform> Configurations { get; set; }

        public override string ToString()
        {
            return $"::SolutionInfo{{SolutionFilePath='{SolutionFilePath}', ProjectFilePath='{ProjectFilePath}', RootSolution={RootSolution}, ProjectFullFilePath='{ProjectFullFilePath}'}}";
        }
    }

    class Program
    {
        const string NewPlatformToolset = "v141_xp";
        const string OldPlatformToolset = "v141_xp";
        const string NewConfigurationSuffix = "_2017";
        static readonly string[] ExcludedPlatforms = new [] { "x64" };

        static readonly string[] mainDirectories = new[]{ @"C:\SVN\External_Libs", @"C:\SVN\Internal_libs\Soft", @"C:\SVN\ReferentNet_7301" }.Select(path => Path.GetLongPath(path)).ToArray();
        static readonly string[] rootSolutions = { "RefNetServer.sln", "Updater.sln", "Referents.sln", "Admin.sln"};

        static bool needNewConfiguration = false;

        public static bool IsNewConfiguration(ConfigurationPlatform configuration)
        {
            return configuration.Configuration.EndsWith(NewConfigurationSuffix);
        }

        public static string ConfigurationNameMutator(ConfigurationPlatform oldConfiguration)
        {
            return needNewConfiguration ?
                (oldConfiguration
                .Configuration
                .Replace("_vc_120_", "")
                .Replace("_vc120_", "")
                .Replace("_vc_120", "")
                .Replace("_vc120", "")
                .Replace("_vs_120_", "")
                .Replace("_vs120_", "")
                .Replace("_vs_120", "")
                .Replace("_vs120", "")
                .Replace("vc_120", "")
                .Replace("vc120", "")
                .Replace("vs_120", "")
                .Replace("vs120", "")
                .Replace("_120_", "")
                .Replace("_120", "")
                .Replace("120_", "")
                .Replace("120", "")
                .Replace("_vc_12_", "")
                .Replace("_vc12_", "")
                .Replace("_vc_12", "")
                .Replace("_vc12", "")
                .Replace("_vs_12_", "")
                .Replace("_vs12_", "")
                .Replace("_vs_12", "")
                .Replace("_vs12", "")
                .Replace("vc_12", "")
                .Replace("vc12", "")
                .Replace("vs_12", "")
                .Replace("vs12", "")
                .Replace("_12_", "")
                .Replace("_12", "")
                .Replace("12_", "")
                .Replace("12", "")
                .Replace(NewConfigurationSuffix, "")
                + NewConfigurationSuffix) : oldConfiguration.Configuration;
        }

        public static string PropertySheetNameChanger(string propertyPath)
        {
            if (propertyPath.ToUpper().Contains("Props.Cpp.".ToUpper()))
                return propertyPath.Replace("vc12", "vc141");
            return propertyPath;
        }

        public static void FillAdditionalOptions(XElement startPosition, string xmlPath)
        {
            AddCmdKey(startPosition, xmlPath, "/Zc:implicitNoexcept-", "/Zc:implicitNoexcept+", "/Zc:implicitNoexcept", "-Zc:implicitNoexcept+", "-Zc:implicitNoexcept-", "-Zc:implicitNoexcept");
            AddCmdKey(startPosition, xmlPath, "/std:c++latest", "/std:c++14", "/std:c++17", "-std:c++14", "-std:c++17", "-std:c++latest", "/std:c++latest");
            AddCmdKey(startPosition, xmlPath, "/Zc:threadSafeInit-", "/Zc:threadSafeInit+", "/Zc:threadSafeInit", "-Zc:threadSafeInit+", "-Zc:threadSafeInit-", "-Zc:threadSafeInit");
        }

        private static void FillPreprocessorDefinitions(SolutionInfo solutionInfo, XElement startPosition, string xmlPath)
        {
            if (solutionInfo.ProjectFilePath.ToLower().Contains("test"))
                UpdatePreprocessorDefinition(startPosition, xmlPath, "_SILENCE_FPOS_SEEKPOS_DEPRECATION_WARNING=1;CPPREST_TARGET_XP;CPPREST_FORCE_PPLX;_SILENCE_TR1_NAMESPACE_DEPRECATION_WARNING=1;_HAS_TR1_NAMESPACE=1;_SILENCE_ALL_CXX17_DEPRECATION_WARNINGS=1;_HAS_AUTO_PTR_ETC=1;_HAS_TR2_SYS_NAMESPACE=1;_SILENCE_TR2_SYS_NAMESPACE_DEPRECATION_WARNING=1", "CPPREST_TARGET_XP", "CPPREST_FORCE_PPLX", "_HAS_AUTO_PTR_ETC", "_SILENCE_ALL_CXX17_DEPRECATION_WARNINGS", "_SILENCE_TR2_SYS_NAMESPACE_DEPRECATION_WARNING", "_HAS_TR2_SYS_NAMESPACE", "BOOST_SYSTEM_NO_DEPRECATED", "_SILENCE_TR1_NAMESPACE_DEPRECATION_WARNING", "_HAS_TR1_NAMESPACE", "_SILENCE_FPOS_SEEKPOS_DEPRECATION_WARNING");
            else
                UpdatePreprocessorDefinition(startPosition, xmlPath, "WINVER=0x0501;_WIN32_WINNT=0x0501;_SILENCE_FPOS_SEEKPOS_DEPRECATION_WARNING=1;CPPREST_TARGET_XP;CPPREST_FORCE_PPLX;_SILENCE_TR1_NAMESPACE_DEPRECATION_WARNING=1;_HAS_TR1_NAMESPACE=1;_SILENCE_ALL_CXX17_DEPRECATION_WARNINGS=1;_HAS_AUTO_PTR_ETC=1;_HAS_TR2_SYS_NAMESPACE=1;_SILENCE_TR2_SYS_NAMESPACE_DEPRECATION_WARNING=1", "WINVER", "_WIN32_WINNT", "CPPREST_TARGET_XP", "CPPREST_FORCE_PPLX", "_HAS_AUTO_PTR_ETC", "_SILENCE_ALL_CXX17_DEPRECATION_WARNINGS", "_SILENCE_TR2_SYS_NAMESPACE_DEPRECATION_WARNING", "_HAS_TR2_SYS_NAMESPACE", "BOOST_SYSTEM_NO_DEPRECATED", "_SILENCE_TR1_NAMESPACE_DEPRECATION_WARNING", "_HAS_TR1_NAMESPACE", "_SILENCE_FPOS_SEEKPOS_DEPRECATION_WARNING");
        }

        static IEnumerable<string> RecursiveDirectory(string directoryPath)
        {
            return new[] { directoryPath }.Concat(Directory.GetDirectories(directoryPath).SelectMany(RecursiveDirectory));
        }

        static readonly string ApplicationDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static readonly string SolutionsFile = Path.Combine(ApplicationDirectory, "allSolutions.txt");
        static readonly string ErrorsFile = Path.Combine(ApplicationDirectory, "errors.txt");
        static readonly string ProjectsFile = Path.Combine(ApplicationDirectory, "allProjects.txt");
        static readonly string UpdatingProjectsConfigurationFile = Path.Combine(ApplicationDirectory, "updatingProjectsConfiguration.txt");
        static readonly string ReallyUpdatedSolutionList = Path.Combine(ApplicationDirectory, "reallyUpdatedSolutionList.txt");
        static readonly string ReallyUpdatedProjectsList = Path.Combine(ApplicationDirectory, "reallyUpdatedProjectsList.txt");
        static readonly string ProjectsOutputsFile = Path.Combine(ApplicationDirectory, "projectsOutputsFile.txt");
        static readonly string ProjectsInputsFile = Path.Combine(ApplicationDirectory, "projectsInputsFile.txt");
        static readonly string CleanCmdFile = Path.Combine(ApplicationDirectory, "clean.cmd");
        static readonly string BuildCmdFile = Path.Combine(ApplicationDirectory, "build.cmd");

        public static List<List<string>> RegexGroupMatchList(string source, Regex regexp, int? forcedMatchCount, params int[] groupsIndex)
        {
            var result = new List<List<string>>();
            var matched = regexp.Matches(source);
            if (!groupsIndex.Any())
                throw new ArgumentException($"{nameof(groupsIndex)} is empty");

            if (forcedMatchCount.HasValue && matched.Count != forcedMatchCount.Value)
            {
                foreach(var _ in Enumerable.Range(1, forcedMatchCount.Value))
                    result.Add(null);
                return result;
            }
            
            foreach (Match submatched in matched)
            {
                var matchesGroups = submatched.Groups;
                if (matchesGroups.Count <= groupsIndex.Max())
                    throw new ArgumentException($"{nameof(groupsIndex)} is not matched to {nameof(regexp)}");
                result.Add(groupsIndex.Select(index => matchesGroups[index].Value).ToList());
            }
            return result;
        }

        static readonly string ProjectInfoStringRegex = @"^Project\(\""(\{[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\})\""\)\s=\s\""(.*)\"",\s\""(.*)\"",\s\""(\{[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\})\""$";
        static readonly Lazy<Regex> ProjectInfoRegex = new Lazy<Regex>(()=>new Regex(ProjectInfoStringRegex, RegexOptions.Compiled), LazyThreadSafetyMode.None);
        static readonly Regex ProjectInfoRegexSelfChecker = new Regex(@"^\s*Project(\s)*\(", RegexOptions.Compiled);
        static readonly string ProjectConfigurationStringRegex = @"\t\t(\{[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\})\.([^|]+)\|([^.]+)\.(Build\.0|ActiveCfg) = ([^|]+)\|([^\n\r]+)(\n\r|\r\n|\n|\r)";
        public static readonly Lazy<Regex> ProjectConfigurationRegex = new Lazy<Regex>(() => new Regex(ProjectConfigurationStringRegex, RegexOptions.Compiled), LazyThreadSafetyMode.None);

        static List<SolutionInfo> GetSlnProjects(string solutionPath)
        {
            var ProjectInfoRegexCache = ProjectInfoRegex.Value;
            var result = new List<SolutionInfo>();
            var slnFile = File.ReadAllLines(solutionPath);
            var slnLineNumber = -1;
            foreach(var slnLine in slnFile)
            {
                ++slnLineNumber;
                if(ProjectInfoRegexSelfChecker.IsMatch(slnLine))
                {
                    var matched = RegexGroupMatchList(slnLine, ProjectInfoRegexCache, 1, 3)[0];
                    if (matched is null)
                        result.Add(new SolutionInfo()
                        {
                            SolutionFilePath = solutionPath,
                            SolutionError = $"Error: Can't get project info from solution '{solutionPath}' for line '{slnLine}' with line number {slnLineNumber}"
                        });
                    else
                    {
                        var projectFilePath = matched[0];
                        result.Add(new SolutionInfo()
                        {
                            SolutionFilePath = solutionPath,
                            ProjectFilePath = projectFilePath,
                            ProjectFullFilePath = Path.GetLongPath(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(solutionPath), projectFilePath)))
                        });
                    }
                }
            }
            return result;
        }

        static void RewriteAllLines(string path, string[] lines)
        {
            if (string.IsNullOrWhiteSpace(path) || path.Length < 5)
                throw new ArgumentException($"{nameof(path)} is wrong");
            if (File.Exists(path))
                File.Delete(path);
            File.WriteAllLines(path, lines);
        }

        static void RewriteAllText(string path, string lines)
        {
            if (string.IsNullOrWhiteSpace(path) || path.Length < 5)
                throw new ArgumentException($"{nameof(path)} is wrong");
            if (File.Exists(path))
                File.Delete(path);
            File.WriteAllText(path, lines);
        }

        static void RewriteAllBytes(string path, byte[] lines)
        {
            if (string.IsNullOrWhiteSpace(path) || path.Length < 5)
                throw new ArgumentException($"{nameof(path)} is wrong");
            if (File.Exists(path))
                File.Delete(path);
            File.WriteAllBytes(path, lines);
        }

        static void WriteAllLines(string path, string[] lines)
        {
            if (string.IsNullOrWhiteSpace(path) || path.Length < 5)
                throw new ArgumentException($"{nameof(path)} is wrong");
            if (!lines.Any())
                return;
            foreach(var line in lines)
                File.AppendAllText(path, line + Environment.NewLine);
        }

        static void WriteLine(string path, string line)
        {
            if (string.IsNullOrWhiteSpace(path) || path.Length < 5)
                throw new ArgumentException($"{nameof(path)} is wrong");
            File.AppendAllText(path, line + Environment.NewLine);
        }

        private static void RewriteLine(string path, string line)
        {
            if (string.IsNullOrWhiteSpace(path) || path.Length < 5)
                throw new ArgumentException($"{nameof(path)} is wrong");
            if (File.Exists(path))
                File.Delete(path);
            File.WriteAllLines(path, new [] { line });
        }

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting ...");
                var mainSubDirectories = mainDirectories.SelectMany(directory => RecursiveDirectory(directory));
                var solutions = mainSubDirectories.SelectMany(directory => Directory.GetFiles(directory, @"*.sln"));
                Console.WriteLine("Solutions list ...");
                var solutionsArray = solutions.ToArray();
                foreach (var file in solutionsArray)
                    Console.WriteLine(file);
                Console.WriteLine("Solutions list save ...");
                RewriteAllLines(SolutionsFile, solutionsArray);
                Console.WriteLine("Getting projects ...");
                var projectsAllInfos = solutionsArray.SelectMany(GetSlnProjects).ToArray();
                Console.WriteLine("Errors of projects parsing saving ...");
                var solutionParsingErrors = projectsAllInfos.Where(info => !(info.SolutionError is null)).Select(info => info.SolutionError);
                RewriteAllLines(ErrorsFile, solutionParsingErrors.ToArray());
                Console.WriteLine("Projects list saving ...");
                var projectsInfos = projectsAllInfos.Where(info => info.SolutionError is null).ToList();
                foreach (var solutionInfo in projectsInfos)
                {
                    if (rootSolutions.Any(rootSlnName => solutionInfo.SolutionFilePath.EndsWith(rootSlnName)))
                        solutionInfo.RootSolution = true;
                }
                var projectsInfosStringed = projectsInfos.Select(info => info.ToString()).ToArray();
                RewriteAllLines(ProjectsFile, projectsInfosStringed);
                Console.WriteLine("Updating configurations ...");
                RewriteLine(UpdatingProjectsConfigurationFile, "Updating configurations ...");
                var remains = projectsInfos.Count();
                foreach (var solutionInfo in projectsInfos)
                {
                    WriteLine(UpdatingProjectsConfigurationFile, "updating configuration for '" + solutionInfo.ProjectFullFilePath + "'. Remains: " + remains);
                    --remains;
                    if (IsUpdatedMainConfigurationProject(solutionInfo))
                        continue;
                    if (!solutionInfo.ProjectFullFilePath.EndsWith(".vcxproj"))
                        continue;
                    if (!File.Exists(solutionInfo.ProjectFullFilePath))
                    {
                        Warning("File not found '" + solutionInfo.ProjectFullFilePath + "'");
                        continue;
                    }
                    if (!System.IO.File.Exists(Path.GetNormalPath(solutionInfo.ProjectFullFilePath)))
                    {
                        Error("File not found by short path '" + solutionInfo.ProjectFullFilePath + "'");
                        continue;
                    }
                    var projXml = XDocument.Load(Path.GetNormalPath(solutionInfo.ProjectFullFilePath));
                    var oldPlatformConfigurations = needNewConfiguration ? GetToolsetConfigurations(OldPlatformToolset, projXml) : new List<ConfigurationPlatform> { };
                    var newExistingConfigurations = needNewConfiguration ? GetConfigurations(IsNewConfiguration, projXml) : GetToolsetConfigurations(NewPlatformToolset, projXml).Concat(GetToolsetConfigurations(OldPlatformToolset, projXml)).ToList();
                    if (!(oldPlatformConfigurations.Any() || newExistingConfigurations.Any()))
                        continue;
                    solutionInfo.ConfigurationUpdateAllowed = true;
                    WriteLine(UpdatingProjectsConfigurationFile, "really updating configuration for '" + solutionInfo.ProjectFullFilePath + "'.");
                    var reliableConfigurations = CloneConfigurationsAndUpdate(solutionInfo, projXml, oldPlatformConfigurations, newExistingConfigurations);
                    if (needNewConfiguration)
                    {
                        GetProjectInputs(solutionInfo, projXml, reliableConfigurations);
                        GetProjectOutputs(solutionInfo, projXml, reliableConfigurations);
                    }
                }
                Console.WriteLine("Projects configurations to solutions update ...");
                foreach (var solutionInfo in projectsInfos.Where(_ => _.Configurations == null).ToList())
                {
                    var projectSourceInfo = projectsInfos
                        .Where(_ => _.Configurations != null && _.ProjectFullFilePath == solutionInfo.ProjectFullFilePath)
                        .FirstOrDefault();
                    solutionInfo.Configurations = projectSourceInfo?.Configurations;
                    solutionInfo.ConfigurationUpdateAllowed = projectSourceInfo?.ConfigurationUpdateAllowed ?? false;
                }
                if (needNewConfiguration)
                {
                    Console.WriteLine("Projects outputs list save ...");
                    RewriteAllLines(ProjectsOutputsFile, projectsInfos.SelectMany(_ => _.ProjectOutputs).ToHashSet().ToArray());
                    Console.WriteLine("Projects inputs list save ...");
                    RewriteAllLines(ProjectsInputsFile, projectsInfos.SelectMany(_ => _.ProjectInputs).ToHashSet().ToArray());
                    Console.WriteLine("Builds scripts save ...");
                    MakeScripts(projectsInfos);
                    Console.WriteLine("Updating solutions ...");
                    UpdatingSolutions(projectsInfos);
                    /*Console.WriteLine("Updating solutions version ...");
                    projectsInfos
                        .Where(_ => _.ConfigurationUpdateAllowed)
                        .Select(_ => _.SolutionFilePath)
                        .Distinct()
                        .Select(_ => new RegexUpdater(_))
                        .Select(_ => _.ReplaceOrAdd(@"(^VisualStudioVersion = \d+\.\d+\.\d+\.\d+$)", @"VisualStudioVersion = 15.0.26228.4"))
                        .Select(_ => _.Apply())
                        .ToList();*/
                }
                Console.WriteLine("Updated projects list save...");
                RewriteAllLines(ReallyUpdatedProjectsList, projectsInfos
                    .Where(_ => _.ConfigurationUpdateAllowed)
                    .Select(_ => _.ProjectFullFilePath)
                    .Distinct()
                    .ToArray());
                Console.WriteLine("Updated solutions list save...");
                RewriteAllLines(ReallyUpdatedSolutionList, projectsInfos
                    .Where(_ => _.ConfigurationUpdateAllowed)
                    .Select(_ => _.SolutionFilePath)
                    .Distinct()
                    .ToArray());
                Console.WriteLine("All done.");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                Error("[FatalError]: " + ex);
            }
            //Console.WriteLine("DebugInfo: " + Environment.NewLine + ProjectInfoStringRegex + Environment.NewLine + ProjectConfigurationStringRegex);
            Console.WriteLine("Press Enter for shutdown.");
            Console.ReadLine();
        }

        private static void UpdatingSolutions(IList<SolutionInfo> projectsInfos)
        {
            var solToConf = new Dictionary<string, HashSet<ConfigurationPlatform>>();
            foreach (var pc in projectsInfos.Where(_ => _.ConfigurationUpdateAllowed && _.Configurations != null))
            {
                if (solToConf.ContainsKey(pc.SolutionFilePath))
                    solToConf[pc.SolutionFilePath].UnionWith(pc.Configurations);
                else
                {
                    var set = new HashSet<ConfigurationPlatform>(pc.Configurations);
                    solToConf.Add(pc.SolutionFilePath, set);
                }
            }
            foreach (var stcs in solToConf)
            {
                var updater = new RegexUpdater(stcs.Key);
                updater.AppendSinceTo(CombinationENL("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution"), "\tEndGlobalSection", SolutionConfigurations(stcs.Value.ToList()));
                updater.UpdateProjectsConfigurationSinceTo(CombinationENL("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution"), "\tEndGlobalSection");
            }
        }

        private static IEnumerable<string> CombinationENL(string v)
        {
            yield return v + "\r\n";
            yield return v + "\n";
        }

        private static string[] SolutionConfigurations(IList<ConfigurationPlatform> configurations)
        {
            return configurations.Select(conf => $"\t\t{conf.ToMinimalMainCondition()} = {conf.ToMinimalMainCondition()}").ToArray();
        }

        private static void GetProjectOutputs(SolutionInfo solutionInfo, XDocument projXml, IList<ConfigurationPlatform> reliableConfigurations)
        {
            foreach(var сonfiguration in reliableConfigurations)
            {
                var importLibrary = projXml
                    .XPathSelectElement($"/*[local-name()='Project']/*[local-name()='ItemDefinitionGroup' and @Condition={сonfiguration.ToMainConditionQuoted()}]/*[local-name()='Link']/*[local-name()='ImportLibrary']") 
                    ?.Value;
                var targetName = projXml
                        .XPathSelectElement($"/*[local-name()='Project']/*[local-name()='PropertyGroup' and @Condition={сonfiguration.ToMainConditionQuoted()}]/*[local-name()='TargetName']")
                        ?.Value
                        ?? projXml.XPathSelectElement($"/*[local-name()='Project']/*[local-name()='PropertyGroup']/*[local-name()='TargetName']")
                        ?.Value;
                var targetNameLib = string.IsNullOrWhiteSpace(targetName) ? null : targetName + ".lib";
                var projectName = projXml
                        .XPathSelectElement($"/*[local-name()='Project']/*[local-name()='PropertyGroup']/*[local-name()='ProjectName']")
                        ?.Value
                        ?? Path.GetFileNameWithoutExtension(solutionInfo.ProjectFilePath);
                var projectNameLib = string.IsNullOrWhiteSpace(projectName) ? null : projectName + ".lib";
                importLibrary = importLibrary ?? targetNameLib ?? projectNameLib;
                var platformToolset = projXml
                        .XPathSelectElement($"/*[local-name()='Project']/*[local-name()='PropertyGroup' and @Condition={сonfiguration.ToMainConditionQuoted()}]/*[local-name()='PlatformToolset']")
                        ?.Value;

                importLibrary = importLibrary
                    .ReplaceIfNotNullOrWhiteSpace("$(TargetName)", targetName)
                    .ReplaceIfNotNullOrWhiteSpace("$(ProjectName)", projectName)
                    .ReplaceIfNotNullOrWhiteSpace("$(Configuration)", сonfiguration.Configuration)
                    .ReplaceIfNotNullOrWhiteSpace("$(Platform)", сonfiguration.Platform)
                    .ReplaceIfNotNullOrWhiteSpace("$(PlatformToolset)", platformToolset);

                if (string.IsNullOrWhiteSpace(importLibrary))
                    continue;

                var outputSplited = importLibrary.Split(new[] { '\\', '/', ')' }, StringSplitOptions.RemoveEmptyEntries);
                if (!outputSplited.Any())
                    continue;

                var output = outputSplited.Last();
                if (string.IsNullOrWhiteSpace(output))
                    continue;

                solutionInfo.ProjectOutputs.Add(output);
            }
        }

        private static void GetProjectInputs(SolutionInfo solutionInfo, XDocument projXml, IList<ConfigurationPlatform> reliableConfigurations)
        {
            var inputs
                = reliableConfigurations
                .SelectMany(configuration
                    => projXml
                    .XPathSelectElements($"/*[local-name()='Project']/*[local-name()='ItemDefinitionGroup' and @Condition={configuration.ToMainConditionQuoted()}]/*[local-name()='Link']/*[local-name()='AdditionalDependencies']")
                    .SelectMany(additionalDependencies => additionalDependencies.Value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(dependence => !dependence.Contains("%(AdditionalDependencies)"))
                    .Select(input =>
                    {
                        var platformToolset = projXml
                            .XPathSelectElement($"/*[local-name()='Project']/*[local-name()='PropertyGroup' and @Condition={configuration.ToMainConditionQuoted()}]/*[local-name()='PlatformToolset']")
                            ?.Value;
                        return
                            input
                            .ReplaceIfNotNullOrWhiteSpace("$(Configuration)", configuration.Configuration)
                            .ReplaceIfNotNullOrWhiteSpace("$(Platform)", configuration.Configuration)
                            .ReplaceIfNotNullOrWhiteSpace("$(PlatformToolset)", platformToolset)
                            .Split(new[] { '\\', '/', ')' }, StringSplitOptions.RemoveEmptyEntries)
                            .Last();
                     })));

            solutionInfo.ProjectInputs.UnionWith(inputs);
        }

        private static void InsertOrUpdateNodeValue(XElement element, string simpleXPath, string insertingValue, Func<string, string> updater)
        {
            if (element is null)
                throw new ArgumentNullException(nameof(element));
            if (simpleXPath is null)
                throw new ArgumentNullException(nameof(simpleXPath));

            var splitedIndex = simpleXPath.IndexOf("/");
            string currentNode = null;
            if (splitedIndex > 0)
            {
                var sourceSimpleXPath = simpleXPath;
                simpleXPath = string.Join(string.Empty, sourceSimpleXPath.Skip(splitedIndex + 1));
                currentNode = string.Join(string.Empty, sourceSimpleXPath.Take(splitedIndex));
            }
            if (splitedIndex == -1)
            {
                currentNode = simpleXPath;
                simpleXPath = string.Empty;
            }
            if (string.IsNullOrEmpty(currentNode))
            {
                element.SetValue(updater(element.Value));
                return;
            }
            var subElement = element.XPathSelectElement($"*[local-name()='{currentNode}']");
            if (subElement is null)
            {
                if (insertingValue is null)
                    return;
                subElement = new XElement(element.Name.Namespace + currentNode);
                element.Add(subElement);
                if (string.IsNullOrEmpty(simpleXPath))
                    subElement.SetValue(insertingValue);
                else
                    InsertOrUpdateNodeValue(subElement, simpleXPath, insertingValue, updater);
            }
            else
            {
                if (string.IsNullOrEmpty(simpleXPath))
                    subElement.SetValue(updater(subElement.Value));
                else
                    InsertOrUpdateNodeValue(subElement, simpleXPath, insertingValue, updater);
            }
        }

        private static void AddCmdKey(XElement element, string simpleXPath, string keyAdded, params string[] keysRemoved)
        {
            var newKey = keyAdded + " %(AdditionalOptions)";
            InsertOrUpdateNodeValue(element, simpleXPath, newKey, source =>
            {
                if (string.IsNullOrWhiteSpace(source))
                    return newKey;
                if (source.Contains(keyAdded))
                    return source;
                foreach (var keyRemoved in keysRemoved)
                    source = source.Replace(keyRemoved, "");
                return keyAdded + " " + source;
            });
        }

        private static void UpdatePreprocessorDefinition(XElement element, string simpleXPath, string keyAdded, params string[] keysRemoved)
        {
            var baseSymbol = "%(PreprocessorDefinitions)";
            var newKey = (string.IsNullOrWhiteSpace(keyAdded) ? "" : (keyAdded + ";")) + baseSymbol;
            InsertOrUpdateNodeValue(element, simpleXPath, newKey, source =>
            {
                if (string.IsNullOrWhiteSpace(source))
                    return newKey;
                var splited = source.Split(new[] { ';' }, StringSplitOptions.None);
                var keysRemovedFull = keysRemoved.Concat(new[] { baseSymbol }).ToList();
                var splitedAfterRemove = splited.Where(key => !keysRemovedFull.Any(removed => key.Contains(removed)));
                if(string.IsNullOrWhiteSpace(keyAdded))
                    keyAdded = string.Join(";", splitedAfterRemove);
                else
                    keyAdded = string.Join(";", new[] {keyAdded}.Concat(splitedAfterRemove));
                return (string.IsNullOrWhiteSpace(keyAdded) ? "" : (keyAdded + ";")) + baseSymbol;
            });
        }

        private static IList<ConfigurationPlatform> CloneConfigurationsAndUpdate(SolutionInfo solutionInfo, XDocument projXml, IList<ConfigurationPlatform> oldPlatformConfigurations, IList<ConfigurationPlatform> newExistingConfigurations)
        {
            if (!oldPlatformConfigurations.Any())
            {
                Warning("Not found old platform configuration");
                foreach (var configuration in newExistingConfigurations)
                    UpdateConfiguration(solutionInfo, configuration, ConfigurationNameMutator(configuration), projXml);
                solutionInfo.Configurations = newExistingConfigurations.Select(configuration => new ConfigurationPlatform(configuration) { Configuration = ConfigurationNameMutator(configuration) }).ToList();
                projXml.Save(Path.GetNormalPath(solutionInfo.ProjectFullFilePath));
                return newExistingConfigurations;
            }

            if (newExistingConfigurations.Any())
                foreach (var configuration in newExistingConfigurations)
                    DropConfiguration(configuration, projXml);
            foreach (var oldConfiguration in oldPlatformConfigurations)
                CloneConfiguration(oldConfiguration, ConfigurationNameMutator(oldConfiguration), projXml);
            var newConfigurations = GetConfigurations(IsNewConfiguration, projXml);
            foreach (var configuration in newConfigurations)
                UpdateConfiguration(solutionInfo, configuration, ConfigurationNameMutator(configuration), projXml);
            solutionInfo.Configurations = newConfigurations.Select(configuration => new ConfigurationPlatform(configuration) { Configuration = ConfigurationNameMutator(configuration) } ).ToList();
            projXml.Save(Path.GetNormalPath(solutionInfo.ProjectFullFilePath));
            return oldPlatformConfigurations;
        }

        private static void UpdateConfiguration(SolutionInfo solutionInfo, ConfigurationPlatform configuration, string newConfigurationName, XDocument projXml)
        {
            var updatingNodes = GetConfigurationNodes(configuration, projXml);
            var newConfiguration = new ConfigurationPlatform(configuration) { Configuration = newConfigurationName };
            bool PropertyGroup_Configuration_handled = false;
            bool ItemDefinitionGroup_handled = false;
            foreach (var xmlNode in updatingNodes)
            {
                switch (xmlNode.Type)
                {
                    case NodeType.ProjectConfiguration:
                        xmlNode.Node.SetAttributeValue("Include", newConfiguration.ToMinimalMainCondition());
                        xmlNode.Node.SetElementValue(xmlNode.Node.Name.Namespace + "Configuration", newConfiguration.Configuration);
                        break;
                    case NodeType.ItemGroup_ClCompile_subitem:
                    case NodeType.ItemGroup_Link_subitem:
                    case NodeType.ItemGroup_Unclassified_subitem:
                    case NodeType.PropertyGroup_LabelIsNotConfiguration:
                    case NodeType.PropertyGroup_Unclassified_subitems:
                    case NodeType.PropertyGroup_TargetName_subitem:
                        break;
                    case NodeType.ImportGroup_LabelIsAny:
                        var propertySheetNodes = 
                            xmlNode
                            .Node
                            .XPathSelectElements("*[local-name()='Import' and @Project]")
                            .Where(importNode => importNode.Attribute("Project").Value.ToUpper().EndsWith(".props".ToUpper()));
                        foreach (var propertySheetNode in propertySheetNodes)
                            propertySheetNode.SetAttributeValue("Project", PropertySheetNameChanger(propertySheetNode.Attribute("Project").Value));
                        break;
                    case NodeType.PropertyGroup_Configuration:
                        PropertyGroup_Configuration_handled = true;
                        xmlNode.Node.SetElementValue(xmlNode.Node.Name.Namespace + "PlatformToolset", NewPlatformToolset);
                        break;
                    case NodeType.ItemDefinitionGroup:
                        ItemDefinitionGroup_handled = true;
                        FillAdditionalOptions(xmlNode.Node, @"ClCompile/AdditionalOptions");
                        FillPreprocessorDefinitions(solutionInfo, xmlNode.Node, @"ClCompile/PreprocessorDefinitions");
                        //updateOption(xmlNode.Node, @"ClCompile/ProgramDataBaseFileName", path=>path.Replace("VC100", "$(PlatformToolset)").Replace("vc100", "$(PlatformToolset)"));
                        break;
                    case NodeType.PropertyGroup_OutDir_subitem:
                    case NodeType.PropertyGroup_IntDir_subitem:
                        //updateOption(xmlNode.Node, @"", path => path.Replace("vc10", "$(PlatformToolset)").Replace("VC10", "$(PlatformToolset)"));
                        break;
                    case NodeType.ItemGroup_ClCompile_AdditionalOptions_subitem:
                        FillAdditionalOptions(xmlNode.Node, @"");
                        break;
                    case NodeType.ItemGroup_ClCompile_PreprocessorDefinitions_subitem:
                        FillPreprocessorDefinitions(solutionInfo, xmlNode.Node, @"");
                        break;
                    default:
                        throw new NotImplementedException(nameof(NodeType));
                }
            }
            if(!ItemDefinitionGroup_handled)
            {
                var insertAfter = projXml.XPathSelectElement($"/*[local-name()='Project']/*[local-name()='ItemGroup' and @Label={Q("ProjectConfigurations")}]");
                var inserting = new XElement(insertAfter.Name.Namespace + "ItemDefinitionGroup");
                inserting.SetAttributeValue("Condition", configuration.ToMainCondition());
                insertAfter.AddAfterSelf(inserting);
                FillAdditionalOptions(inserting, @"ClCompile/AdditionalOptions");
                FillPreprocessorDefinitions(solutionInfo, inserting, @"ClCompile/PreprocessorDefinitions");
            }
            if (!(PropertyGroup_Configuration_handled))
                throw new NotImplementedException("Not implemented addition of essential xml nodes for force settings changing. to do so.");
        }

        private static void updateOption(XElement node, string simpleXPath, Func<string, string> updater)
        {
            InsertOrUpdateNodeValue(node, simpleXPath, null, updater);
        }

        private static void CloneConfiguration(ConfigurationPlatform oldConfiguration, string newConfigurationName, XDocument projXml)
        {
            var nodes = GetConfigurationNodes(oldConfiguration, projXml);
            var newConfiguration = new ConfigurationPlatform (oldConfiguration) { Configuration=newConfigurationName };
            foreach (var xmlNode in nodes)
            {
                var newNode = new XElement(xmlNode.Node);
                xmlNode.Node.AddAfterSelf(newNode);
                xmlNode.Node = newNode;
                switch (xmlNode.Type)
                {
                    case NodeType.ProjectConfiguration:
                        xmlNode.Node.SetAttributeValue("Include", newConfiguration.ToMinimalMainCondition());
                        xmlNode.Node.SetElementValue(xmlNode.Node.Name.Namespace + "Configuration", newConfiguration.Configuration);
                        break;
                    case NodeType.PropertyGroup_Configuration:
                    case NodeType.ItemDefinitionGroup:
                    case NodeType.ImportGroup_LabelIsAny:
                    case NodeType.PropertyGroup_LabelIsNotConfiguration:
                    case NodeType.ItemGroup_ClCompile_AdditionalOptions_subitem:
                    case NodeType.ItemGroup_ClCompile_PreprocessorDefinitions_subitem:
                    case NodeType.ItemGroup_ClCompile_subitem:
                    case NodeType.ItemGroup_Link_subitem:
                    case NodeType.ItemGroup_Unclassified_subitem:
                    case NodeType.PropertyGroup_Unclassified_subitems:
                    case NodeType.PropertyGroup_IntDir_subitem:
                    case NodeType.PropertyGroup_OutDir_subitem:
                    case NodeType.PropertyGroup_TargetName_subitem:
                        xmlNode.Node.SetAttributeValue("Condition", newConfiguration.ToMainCondition());
                        break;
                    default:
                        throw new NotImplementedException(nameof(NodeType));
                }
            }
        }

        private static void DropConfiguration(ConfigurationPlatform configuration, XDocument projXml)
        {
            var xmlNodes = GetConfigurationNodes(configuration, projXml);
            foreach (var node in xmlNodes)
                node.Node.Remove();
        }

        private static string Q(string value)
        {
            return @"""" + value + @"""";
        }

        private static IList<TypeToNode> GetConfigurationNodes(ConfigurationPlatform configuration, XDocument projXml)
        {
            //var a = $"/*[local-name()='Project']/*[local-name()='PropertyGroup' and @Condition={configuration.ToMainConditionQuoted()} and @Label={Q("Configuration")}]";
            return
                projXml
                    .XPathSelectElements($"/*[local-name()='Project']/*[local-name()='ItemGroup' and @Label={Q("ProjectConfigurations")}]/*[local-name()='ProjectConfiguration' and @Include={Q(configuration.ToMinimalMainCondition())}]")
                    .Select(node => new TypeToNode { Type = NodeType.ProjectConfiguration, Node = node })
                /*projXml
                .XPathSelectElements($"/*[local-name()='Project']/*[local-name()='ItemGroup' and @Label={Q("ProjectConfigurations")}]/*[local-name()='ProjectConfiguration']")
                .Where(projectConfiguration =>
                    projectConfiguration.XPathSelectElement(@"*[local-name()='Configuration']").Value == configuration.Configuration
                    && projectConfiguration.XPathSelectElement(@"*[local-name()='Platform']").Value == configuration.Platform)
                .Select(node => new TypeToNode { Type = NodeType.ProjectConfiguration, Node = node })*/
                .Concat(projXml
                    .XPathSelectElements($"/*[local-name()='Project']/*[local-name()='PropertyGroup' and @Condition={configuration.ToMainConditionQuoted()} and @Label={Q("Configuration")}]")
                    .Select(node => new TypeToNode { Type = NodeType.PropertyGroup_Configuration, Node = node }))
                .Concat(projXml
                    .XPathSelectElements($"/*[local-name()='Project']/*[local-name()='PropertyGroup' and @Condition={configuration.ToMainConditionQuoted()} and not(@Label={Q("Configuration")})]")
                    .Select(node => new TypeToNode { Type = NodeType.PropertyGroup_LabelIsNotConfiguration, Node = node }))
                .Concat(projXml
                    .XPathSelectElements($"/*[local-name()='Project']/*[local-name()='PropertyGroup']/*[local-name()='OutDir' and @Condition={configuration.ToMainConditionQuoted()}]")
                    .Select(node => new TypeToNode { Type = NodeType.PropertyGroup_OutDir_subitem, Node = node }))
                .Concat(projXml
                    .XPathSelectElements($"/*[local-name()='Project']/*[local-name()='PropertyGroup']/*[local-name()='IntDir' and @Condition={configuration.ToMainConditionQuoted()}]")
                    .Select(node => new TypeToNode { Type = NodeType.PropertyGroup_IntDir_subitem, Node = node }))
                .Concat(projXml
                    .XPathSelectElements($"/*[local-name()='Project']/*[local-name()='PropertyGroup']/*[local-name()='TargetName' and @Condition={configuration.ToMainConditionQuoted()}]")
                    .Select(node => new TypeToNode { Type = NodeType.PropertyGroup_TargetName_subitem, Node = node }))
                .Concat(projXml
                    .XPathSelectElements($"/*[local-name()='Project']/*[local-name()='PropertyGroup']/*[local-name()!='IntDir' and local-name()!='OutDir' and local-name()!='TargetName' and @Condition={configuration.ToMainConditionQuoted()}]")
                    .Select(node => new TypeToNode { Type = NodeType.PropertyGroup_Unclassified_subitems, Node = node }))
                .Concat(projXml
                    .XPathSelectElements($"/*[local-name()='Project']/*[local-name()='ItemDefinitionGroup' and @Condition={configuration.ToMainConditionQuoted()}]")
                    .Select(node => new TypeToNode { Type = NodeType.ItemDefinitionGroup, Node = node }))
                .Concat(projXml
                    .XPathSelectElements($"/*[local-name()='Project']/*[local-name()='ImportGroup' and @Condition={configuration.ToMainConditionQuoted()}]")
                    .Select(node => new TypeToNode { Type = NodeType.ImportGroup_LabelIsAny, Node = node }))
                .Concat(projXml
                    .XPathSelectElements($"/*[local-name()='Project']/*[local-name()='ItemGroup']/*[local-name()='ClCompile']/*[local-name()='AdditionalOptions' and @Condition={configuration.ToMainConditionQuoted()}]")
                    .Select(node => new TypeToNode { Type = NodeType.ItemGroup_ClCompile_AdditionalOptions_subitem, Node = node }))
                .Concat(projXml
                    .XPathSelectElements($"/*[local-name()='Project']/*[local-name()='ItemGroup']/*[local-name()='ClCompile']/*[local-name()='PreprocessorDefinitions' and @Condition={configuration.ToMainConditionQuoted()}]")
                    .Select(node => new TypeToNode { Type = NodeType.ItemGroup_ClCompile_PreprocessorDefinitions_subitem, Node = node }))
                .Concat(projXml
                    .XPathSelectElements($"/*[local-name()='Project']/*[local-name()='ItemGroup']/*[local-name()='ClCompile']/*[local-name()!='AdditionalOptions' and local-name()!='PreprocessorDefinitions' and @Condition={configuration.ToMainConditionQuoted()}]")
                    .Select(node => new TypeToNode { Type = NodeType.ItemGroup_ClCompile_subitem, Node = node }))
                .Concat(projXml
                    .XPathSelectElements($"/*[local-name()='Project']/*[local-name()='ItemGroup']/*[local-name()='Link']/*[@Condition={configuration.ToMainConditionQuoted()}]")
                    .Select(node => new TypeToNode { Type = NodeType.ItemGroup_Link_subitem, Node = node }))
                .Concat(projXml
                    .XPathSelectElements($"/*[local-name()='Project']/*[local-name()='ItemGroup']/*[local-name()!='ClCompile' and local-name()!='Link']/*[@Condition={configuration.ToMainConditionQuoted()}]")
                    .Select(node => new TypeToNode { Type = NodeType.ItemGroup_Unclassified_subitem, Node = node }))
                .ToList();
        }

        private static IList<ConfigurationPlatform> GetToolsetConfigurations(string toolsetName, XDocument projXml)
        {
            var allConfigurations = GetAllConfigurations(projXml);
            if (!allConfigurations.Any())
            {
                //Warning("Not found any configuration");
                return new List<ConfigurationPlatform>();
            }
            return allConfigurations.Where(configuration =>
                projXml.XPathSelectElements($"/*[local-name()='Project']/*[local-name()='PropertyGroup' and @Condition={configuration.ToMainConditionQuoted()} and @Label={Q("Configuration")}]/*[local-name()='PlatformToolset']")
                    .Any(platformToolset => platformToolset.Value == toolsetName)).ToList();
        }

        private static IList<ConfigurationPlatform> GetConfigurations(Func<ConfigurationPlatform, bool> findingConfiguration, XDocument projXml)
        {
            var projectConfigurations =
                projXml
                .XPathSelectElements($"/*[local-name()='Project']/*[local-name()='ItemGroup' and @Label={Q("ProjectConfigurations")}]/*[local-name()='ProjectConfiguration']");
            return 
                projectConfigurations
                .Select(projectConfiguration =>
                new ConfigurationPlatform ()
                {
                    Configuration = projectConfiguration.XPathSelectElement(@"*[local-name()='Configuration']").Value,
                    Platform = projectConfiguration.XPathSelectElement(@"*[local-name()='Platform']").Value
                })
                .Where(_ => !ExcludedPlatforms.Contains(_.Platform))
                .Where(findingConfiguration)
                .ToList();
        }

        private static IList<ConfigurationPlatform> GetAllConfigurations(XDocument projXml)
        {
            var projectConfigurations = projXml.XPathSelectElements(@"/*[local-name()='Project']/*[local-name()='ItemGroup' and @Label=""ProjectConfigurations""]/*[local-name()='ProjectConfiguration']");
            return projectConfigurations.Select(projectConfiguration =>
            new ConfigurationPlatform
            {
                Configuration = projectConfiguration.XPathSelectElement(@"*[local-name()='Configuration']").Value,
                Platform = projectConfiguration.XPathSelectElement(@"*[local-name()='Platform']").Value
            })
            .Where(_=>!ExcludedPlatforms.Contains(_.Platform))
            .ToList();
        }

        private static void Error(string lineForLog)
        {
            WriteLine(ErrorsFile, "[Error]: '" + lineForLog+"'");
        }

        private static void Warning(string lineForLog)
        {
            WriteLine(ErrorsFile, "[Warning]: '" + lineForLog+"'");
        }

        private static bool IsUpdatedMainConfigurationProject(SolutionInfo solutionInfo)
        {
            if (!SolutionInfo.UpdatedMainConfigurationProjectFullFilePath.Contains(solutionInfo.ProjectFullFilePath))
            {
                SolutionInfo.UpdatedMainConfigurationProjectFullFilePath.Add(solutionInfo.ProjectFullFilePath);
                return false;
            }
            return true;
        }

        class SolutionDeps
        {
            public HashSet<string> Input { get; set; }
            public HashSet<string> Output { get; set; }
            public HashSet<ConfigurationPlatform> Configuration { get; set; }
        }

        private static void MakeScripts(IList<SolutionInfo> infoSource)
        {
            var solDeps = new Dictionary<string, SolutionDeps>();
            foreach(var sol in infoSource.Where(_=>_.ConfigurationUpdateAllowed).Select(_ => _.SolutionFilePath).Distinct())
            {
                var solOut = infoSource.Where(_ => _.SolutionFilePath == sol).SelectMany(_=>_.ProjectOutputs).Distinct();
                var solIn = infoSource.Where(_ => _.SolutionFilePath == sol).SelectMany(_ => _.ProjectInputs).Distinct();
                solIn = solIn.Except(solOut);
                var confs = infoSource.Where(_ => _.SolutionFilePath == sol).Where(_ => _.Configurations != null).SelectMany(_ => _.Configurations).Distinct();
                solDeps.Add(sol, new SolutionDeps() { Input = solIn.ToHashSet(), Output = solOut.ToHashSet(), Configuration = confs.ToHashSet() });
            }

            var rootDeps = new Dictionary<string, SolutionDeps>();
            foreach (var sol in solDeps.Keys)
            {
                if (rootSolutions.Any(rootSlnName => sol.EndsWith(rootSlnName)))
                    rootDeps.Add(sol, solDeps[sol]);
            }

            var reachableDeps = rootDeps.ToDictionary(entry => entry.Key, entry => entry.Value);
            var newReachableDeps = rootDeps.ToDictionary(entry => entry.Key, entry => entry.Value);
            int oldReachableDepsAmount;
            do 
            {
                oldReachableDepsAmount = reachableDeps.Count;
                var oldNewReachableDeps = newReachableDeps.ToList();
                newReachableDeps.Clear();
                var unreachableDepsCandidates = solDeps.Except(reachableDeps);
                foreach (var rsol in oldNewReachableDeps)
                    foreach (var rsolInput in rsol.Value.Input)
                        foreach (var reachedUnreached in unreachableDepsCandidates.Where(usol => usol.Value.Output.Contains(rsolInput)))
                        {
                            if(!newReachableDeps.ContainsKey(reachedUnreached.Key))
                                newReachableDeps.Add(reachedUnreached.Key, reachedUnreached.Value);
                        }
                foreach(var dep in newReachableDeps)
                    if (!reachableDeps.ContainsKey(dep.Key))
                        reachableDeps.Add(dep.Key, dep.Value);
            } while (oldReachableDepsAmount!=reachableDeps.Count);

            var unbuildableDeps = reachableDeps
                .SelectMany(_ => _.Value.Input)
                .Except(reachableDeps.SelectMany(_ => _.Value.Output))
                .ToHashSet();

            var clean = new StringBuilder();
            var build = new StringBuilder();

            var buildedOutput = unbuildableDeps.ToHashSet();
            foreach (var lib in unbuildableDeps)
            {
                MakeClean(lib, clean);
                MakeBuild(lib, build);
            }

            var buildedDeps = new Dictionary<string, SolutionDeps>();

            Prolog(clean);
            Prolog(build);

            do 
            {
                foreach(var sol in reachableDeps.Except(buildedDeps).ToDictionary(entry => entry.Key, entry => entry.Value))
                {
                    if(sol.Value.Input.All(_=>buildedOutput.Contains(_)))
                    {
                        buildedOutput.UnionWith(sol.Value.Output);
                        buildedDeps.Add(sol.Key, sol.Value);
                        MakeClean(sol, clean);
                        MakeBuild(sol, build);
                    }
                }
            } while (buildedDeps.Count!=reachableDeps.Count);

            Epilog(clean);
            Epilog(build);

            RewriteAllBytes(CleanCmdFile, Encoding.ASCII.GetBytes(clean.ToString()));
            RewriteAllBytes(BuildCmdFile, Encoding.ASCII.GetBytes(build.ToString()));
        }

        private static void Epilog(StringBuilder output)
        {
            output.AppendLine(" && echo OK");
            output.AppendLine("pause");
        }

        private static void Prolog(StringBuilder output)
        {
            output.AppendLine("echo Starting");
            output.AppendLine("echo Started ^");
        }

        private static void MakeBuild(KeyValuePair<string, SolutionDeps> sol, StringBuilder output)
        {
            foreach (var conf in sol.Value.Configuration)
                output.AppendLine($" && MSBuild /t:Build {Q(Path.GetNormalPath(sol.Key))} /p:Configuration={Q(conf.Configuration)} /p:Platform={Q(conf.Platform)} ^");
            output.AppendLine("^");
        }

        private static void MakeClean(KeyValuePair<string, SolutionDeps> sol, StringBuilder output)
        {
            foreach (var conf in sol.Value.Configuration)
                output.AppendLine($" && MSBuild /t:Clean {Q(Path.GetNormalPath(sol.Key))} /p:Configuration={Q(conf.Configuration)} /p:Platform={Q(conf.Platform)} ^");
            output.AppendLine("^");
        }

        private static void MakeBuild(string lib, StringBuilder output)
        {
            output.AppendLine("::build "+lib);
        }

        private static void MakeClean(string lib, StringBuilder output)
        {
            output.AppendLine("::clean " + lib);
        }
    }
}