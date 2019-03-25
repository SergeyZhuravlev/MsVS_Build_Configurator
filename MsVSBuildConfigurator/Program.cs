using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Delimon.Win32.IO;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;

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

    public class RegexUpdater
    {
        private string _updatedConfigPath;
        private List<Tuple<Regex, string>> _regexUpdaters;

        public RegexUpdater(string updatedConfigPath)
        {
            _updatedConfigPath = updatedConfigPath;
            _regexUpdaters = new List<Tuple<Regex, string>>();
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
    }

    public enum NodeType
    {
        ProjectConfigurations = 1
        , PropertyGroup_Configuration
        , PropertyGroup_LabelIsNotConfiguration
        , ItemDefinitionGroup
        , ImportGroup_LabelIsAny
        , ItemGroup_ClCompile_CompileAsManaged
        , ItemGroup_ClCompile_PrecompiledHeader
    }

    public class TypeToNode
    {
        public NodeType Type { get; set; }
        public XElement Node { get; set; }
    }

    public class ConfigurationPlatform
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

        public string ToMainConditionQuoted()
        {
            return @"""" + $"'$(Configuration)|$(Platform)' =='{Configuration}|{Platform}\'" + @"""";
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

        public override string ToString()
        {
            return $"::SolutionInfo{{SolutionFilePath='{SolutionFilePath}', ProjectFilePath='{ProjectFilePath}', RootSolution={RootSolution}, ProjectFullFilePath='{ProjectFullFilePath}'}}";
        }
    }

    class Program
    {
        const string NewPlatformToolset = "v141_xp";

        static IEnumerable<string> RecursiveDirectory(string directoryPath)
        {
            return new[] { directoryPath }.Concat(Directory.GetDirectories(directoryPath).SelectMany(RecursiveDirectory));
        }

        static readonly string ApplicationDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static readonly string SolutionsFile = Path.Combine(ApplicationDirectory, "allSolutions.txt");
        static readonly string ErrorsFile = Path.Combine(ApplicationDirectory, "errors.txt");
        static readonly string ProjectsFile = Path.Combine(ApplicationDirectory, "allProjects.txt");
        static readonly string UpdatingProjectsConfigurationFile = Path.Combine(ApplicationDirectory, "updatingProjectsConfiguration.txt");

        static List<string> RegexGroupMatchList(string source, Regex regexp, params int[] groupsIndex)
        {
            var matched = regexp.Matches(source);
            if (!groupsIndex.Any())
                throw new ArgumentException($"{nameof(groupsIndex)} is empty");
            if (matched.Count != 1)
                return null;
            var matchesGroups = matched[0].Groups;
            if (matchesGroups.Count <= groupsIndex.Max())
                return null;
            return groupsIndex.Select(index => matchesGroups[index].Value).ToList();
        }

        static readonly string ProjectInfoStringRegex = @"^Project\(\""(\{[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\})\""\)\s=\s\""(.*)\"",\s\""(.*)\"",\s\""(\{[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\})\""$";
        static readonly Lazy<Regex> ProjectInfoRegex = new Lazy<Regex>(()=>new Regex(ProjectInfoStringRegex, RegexOptions.Compiled), LazyThreadSafetyMode.None);
        static readonly Regex ProjectInfoRegexSelfChecker = new Regex(@"^\s*Project(\s)*\(", RegexOptions.Compiled);

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
                    var matched = RegexGroupMatchList(slnLine, ProjectInfoRegexCache, 3);
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
                            ProjectFullFilePath = /*Path.GetLongPath(*/Path.GetFullPath(Path.Combine(Path.GetDirectoryName(solutionPath), projectFilePath))/*)*/
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
            WriteAllLines(path, lines);
        }

        static void WriteAllLines(string path, string[] lines)
        {
            if (string.IsNullOrWhiteSpace(path) || path.Length < 5)
                throw new ArgumentException($"{nameof(path)} is wrong");
            if (!lines.Any())
                return;
            File.WriteAllLines(path, lines);
        }

        static void WriteLine(string path, string line)
        {
            if (string.IsNullOrWhiteSpace(path) || path.Length < 5)
                throw new ArgumentException($"{nameof(path)} is wrong");
            File.WriteAllLines(path, new[] { line });
        }

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting ...");
                var mainDirectories = new[] { @"C:\SVN\External_Libs", @"C:\SVN\Internal_libs\Soft", @"C:\SVN\ReferentNet" }.Select(path => /*Path.GetLongPath(*/path/*)*/);
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
                var rootSolutions = new[] { "ControlService.sln", "Launcher.sln", "RefNetServer.sln", "Updater.sln", "Referents.sln", "Admin.sln", "FirebirdServiceInstaller.sln" };
                foreach (var solutionInfo in projectsInfos)
                {
                    if (rootSolutions.Any(rootSlnName => solutionInfo.SolutionFilePath.EndsWith(rootSlnName)))
                        solutionInfo.RootSolution = true;
                }
                var projectsInfosStringed = projectsInfos.Select(info=> info.ToString()).ToArray();
                RewriteAllLines(ProjectsFile, projectsInfosStringed);
                Console.WriteLine("Updating configurations ...");
                var remains = projectsInfos.Count();
                foreach (var solutionInfo in projectsInfos)
                {
                    WriteLine(UpdatingProjectsConfigurationFile, "updating configuration for '" + solutionInfo.ProjectFullFilePath + "'. Remains: " + remains);
                    --remains;
                    if (IsUpdatedMainConfigurationProject(solutionInfo))
                        continue;
                    if (!solutionInfo.ProjectFullFilePath.EndsWith(".vcxproj"))
                        continue;
                    WriteLine(UpdatingProjectsConfigurationFile, "realy updating configuration for '" + solutionInfo.ProjectFullFilePath + "'.");
                    CloneConfigurationsAndUpdate(solutionInfo, "v120_xp", "Release_2017");
                }
                Console.WriteLine("Updating solutions ...");
                projectsInfos.Select(_ => _.SolutionFilePath)
                    .Distinct()
                    .Select(_ => new RegexUpdater(_))
                    .Select(_ => _.ReplaceOrAdd(@"^VisualStudioVersion = \d+\.\d+\.\d+\.\d+$", @"VisualStudioVersion = 15.0.26228.4"))
                    .Select(_ => _.Apply())
                    .ToList();
                Console.WriteLine("All done.");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                Error("[FatalError]: " + ex);
            }
            Console.WriteLine("DebugInfo: " + Environment.NewLine + ProjectInfoStringRegex);
            Console.WriteLine("Press Enter for shutdown.");
            Console.ReadLine();
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
                simpleXPath = string.Join(string.Empty, simpleXPath.Skip(splitedIndex + 1));
                currentNode = string.Join(string.Empty, simpleXPath.Take(splitedIndex));
            }
            if (string.IsNullOrEmpty(currentNode))
                throw new ArgumentException(nameof(simpleXPath));

            var subElement = element.XPathSelectElement(currentNode);
            if (subElement is null)
            {
                subElement = new XElement(currentNode);
                element.Add(subElement);
                if (string.IsNullOrEmpty(simpleXPath))
                    subElement.SetValue(insertingValue);
                else
                    InsertOrUpdateNodeValue(subElement, simpleXPath, insertingValue, updater);
            }
            else
            {
                if (string.IsNullOrEmpty(simpleXPath))
                    subElement.SetValue(insertingValue);
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

        private static void CloneConfigurationsAndUpdate(SolutionInfo solutionInfo, string oldToolsetName, string newConfigurationName)
        {
            var projXml = XDocument.Load(solutionInfo.ProjectFullFilePath);
            var oldPlatformConfigurations = GetToolsetConfigurations(oldToolsetName, projXml);
            var newExistingConfigurations = GetConfigurations(newConfigurationName, projXml);
            if (!oldPlatformConfigurations.Any())
            {
                if (!newExistingConfigurations.Any())
                {
                    Error("Not found old platform configuration and new platform configuration");
                    return;
                }
                Warning("Not found old platform configuration");
                foreach(var configuration in newExistingConfigurations)
                    UpdateConfiguration(configuration, projXml);
                projXml.Save(solutionInfo.ProjectFullFilePath);
                return;
            }
            if (newExistingConfigurations.Any())
                foreach (var configuration in newExistingConfigurations)
                    DropConfiguration(configuration, projXml);
            foreach (var oldConfiguration in oldPlatformConfigurations)
                CloneConfiguration(oldConfiguration, newConfigurationName, projXml);
            var newConfigurations = GetConfigurations(newConfigurationName, projXml);
            foreach (var configuration in newConfigurations)
                UpdateConfiguration(configuration, projXml);
            projXml.Save(solutionInfo.ProjectFullFilePath);
        }

        private static void UpdateConfiguration(ConfigurationPlatform configuration, XDocument projXml)
        {
            var updatingNodes = GetConfigurationNodes(configuration, projXml);
            foreach (var xmlNode in updatingNodes)
            {
                switch (xmlNode.Type)
                {
                    case NodeType.ProjectConfigurations:
                    case NodeType.ItemGroup_ClCompile_CompileAsManaged:
                    case NodeType.ItemGroup_ClCompile_PrecompiledHeader:
                    case NodeType.ImportGroup_LabelIsAny:
                    case NodeType.PropertyGroup_LabelIsNotConfiguration:
                        break;
                    case NodeType.PropertyGroup_Configuration:
                        xmlNode.Node.SetElementValue("PlatformToolset", NewPlatformToolset);
                        break;
                    case NodeType.ItemDefinitionGroup:
                        AddCmdKey(xmlNode.Node, @"ClCompile/AdditionalOptions", "/Zc:implicitNoexcept-", "/Zc:implicitNoexcept");
                        break;
                    default:
                        throw new NotImplementedException(nameof(NodeType));
                }
            }
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
                    case NodeType.ProjectConfigurations:
                        xmlNode.Node.SetElementValue("Configuration", newConfigurationName);
                        break;
                    case NodeType.PropertyGroup_Configuration:
                    case NodeType.ItemDefinitionGroup:
                    case NodeType.ImportGroup_LabelIsAny:
                    case NodeType.PropertyGroup_LabelIsNotConfiguration:
                    case NodeType.ItemGroup_ClCompile_CompileAsManaged:
                    case NodeType.ItemGroup_ClCompile_PrecompiledHeader:
                        xmlNode.Node.SetAttributeValue("Condition", newConfiguration.ToMainConditionQuoted());
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

        private static IList<TypeToNode> GetConfigurationNodes(ConfigurationPlatform configuration, XDocument projXml)
        {
            return
                projXml
                    .XPathSelectElements($"/Project/ItemGroup[@Label='ProjectConfigurations']/ProjectConfiguration[./Configuration='{configuration.Configuration}' and ./Platform='{configuration.Platform}']")
                    .Select(node => new TypeToNode { Type = NodeType.ProjectConfigurations, Node = node })
                .Concat(projXml
                    .XPathSelectElements($"/Project/PropertyGroup[@Condition={configuration.ToMainConditionQuoted()} and @Label='Configuration']")
                    .Select(node => new TypeToNode { Type = NodeType.PropertyGroup_Configuration, Node = node }))
                .Concat(projXml
                    .XPathSelectElements($"/Project/PropertyGroup[@Condition={configuration.ToMainConditionQuoted()} and not(@Label='Configuration')]")
                    .Select(node => new TypeToNode { Type = NodeType.PropertyGroup_LabelIsNotConfiguration, Node = node }))
                .Concat(projXml
                    .XPathSelectElements($"/Project/ItemDefinitionGroup[@Condition={configuration.ToMainConditionQuoted()}]")
                    .Select(node => new TypeToNode { Type = NodeType.ItemDefinitionGroup, Node = node }))
                .Concat(projXml
                    .XPathSelectElements($"/Project/ImportGroup[@Condition={configuration.ToMainConditionQuoted()}]")
                    .Select(node => new TypeToNode { Type = NodeType.ImportGroup_LabelIsAny, Node = node }))
                .Concat(projXml
                    .XPathSelectElements($"/Project/ItemGroup/ClCompile/CompileAsManaged[@Condition={configuration.ToMainConditionQuoted()}]")
                    .Select(node => new TypeToNode { Type = NodeType.ItemGroup_ClCompile_CompileAsManaged, Node = node }))
                .Concat(projXml
                    .XPathSelectElements($"/Project/ItemGroup/ClCompile/PrecompiledHeader[@Condition={configuration.ToMainConditionQuoted()}]")
                    .Select(node => new TypeToNode { Type = NodeType.ItemGroup_ClCompile_PrecompiledHeader, Node = node }))
                .ToList();
        }

        private static IList<ConfigurationPlatform> GetToolsetConfigurations(string toolsetName, XDocument projXml)
        {
            var allConfigurations = GetAllConfigurations(projXml);
            if (!allConfigurations.Any())
            {
                Error("Not found any configuration");
                throw new Exception("project file without any configuration");
            }
            return allConfigurations.Where(configuration =>
                projXml.XPathSelectElements($"/Project/PropertyGroup[@Condition={configuration.ToMainConditionQuoted()} and @Label='Configuration']/PlatformToolset")
                    .Any(platformToolset => platformToolset.Value == toolsetName)).ToList();
        }

        private static IList<ConfigurationPlatform> GetConfigurations(string configurationName, XDocument projXml)
        {
            var projectConfigurations = projXml.XPathSelectElements($"/Project/ItemGroup[@Label='ProjectConfigurations']/ProjectConfiguration[./Configuration='{configurationName}']");
            return projectConfigurations.Select(projectConfiguration =>
            new ConfigurationPlatform ()
            {
                Configuration = projectConfiguration.Element(@"Configuration").Value,
                Platform = projectConfiguration.Element(@"Platform").Value
            }).ToList();
        }

        private static IList<ConfigurationPlatform> GetAllConfigurations(XDocument projXml)
        {
            var projectConfigurations = projXml.XPathSelectElements($"/Project/ItemGroup[@Label='ProjectConfigurations']/ProjectConfiguration");
            return projectConfigurations.Select(projectConfiguration =>
            new ConfigurationPlatform
            {
                Configuration = projectConfiguration.Element(@"Configuration").Value,
                Platform = projectConfiguration.Element(@"Platform").Value
            }).ToList();
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
    }
}

