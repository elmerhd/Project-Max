using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using AIMLBot;
using static Max.ServerEngine;
using Newtonsoft.Json;
using OxfordDictionariesAPI.Models;
using System.Linq;
using AIMLBot.Utils;

namespace Max
{
    public class BrainEngine
    {
        public Bot Bot { get; set; }

        public User User { get; set; }

        private MaxEngine MaxEngine;

        public BrainEngine(MaxEngine maxEngine)
        {
            this.MaxEngine = maxEngine;
            Bot = new Bot();
            Bot.loadSettings();
            Bot.loadAIMLFromFiles();
            User = new User(MaxEngine.MaxConfig.DefaultUserName, Bot);
            Bot.isAcceptingUserInput = true;
            Log($"Loading {nameof(BrainEngine)}");
        }

        public string getResponse(string text)
        {
            Request request = new Request(text, User, Bot);
            AIMLBot.Result result = Bot.Chat(request);
            return result.Output;
        }

        public void analyze(ServerResponse serverResponse)
        {
            Log($"Analyzing = {serverResponse.Message}");
            if (App.GetUI() != null)
            {
                App.GetUI().UpdateRecognizedText(serverResponse.Message);
            }
            string response = getResponse(serverResponse.Message);
            MaxEngine.VoiceOutputEngine.Speak(response);
        }

        public void Log(string message)
        {
            Bot.writeToLog(message);
        }
    }
    public class MaxBrain
    {
        public SearchResult SearchResult;
        public string Keyword;
        public Bot Bot;
        public List<LearnPattern> DefinitionPatterns = new List<LearnPattern>();
        public List<LearnPattern> ExamplesPatterns = new List<LearnPattern>();

        public MaxBrain(Bot bot, SearchResult searchResult, string keyword)
        {
            this.SearchResult = searchResult;
            this.Keyword = keyword;
            this.Bot = bot;
            InitDefinitionPatterns();
            InitExamplePatterns();
        }

        public MaxBrain(Bot bot, IList<HtmlNode> nodes, string keyword)
        {
            
        }

        public void InitDefinitionPatterns()
        {
            FileInfo fileInfo = new FileInfo(@"config/definition-patterns.json");
            if (fileInfo.Exists)
            {
                DefinitionPatterns = JsonConvert.DeserializeObject<List<LearnPattern>>(File.ReadAllText(fileInfo.FullName));
            }
            else
            {
                DefinitionPatterns.Add(new LearnPattern("*"));
                DefinitionPatterns.Add(new LearnPattern("what is *"));
                DefinitionPatterns.Add(new LearnPattern("what is a *"));
                DefinitionPatterns.Add(new LearnPattern("what is an *"));
                DefinitionPatterns.Add(new LearnPattern("what is the *"));
                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Ignore;

                using (StreamWriter sw = new StreamWriter(fileInfo.FullName))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, DefinitionPatterns);
                }
            }
        }
        public void InitExamplePatterns()
        {
            FileInfo fileInfo = new FileInfo(@"config/example-patterns.json");
            if (fileInfo.Exists)
            {
                ExamplesPatterns = JsonConvert.DeserializeObject<List<LearnPattern>>(File.ReadAllText(fileInfo.FullName));
            }
            else
            {
                ExamplesPatterns.Add(new LearnPattern("give me an example of sentence using *"));
                ExamplesPatterns.Add(new LearnPattern("can you give me an example of sentence using *"));
                ExamplesPatterns.Add(new LearnPattern("give me an examples of sentence using *"));
                ExamplesPatterns.Add(new LearnPattern("can you give me an example of sentences using *"));
                ExamplesPatterns.Add(new LearnPattern("example of sentence using *"));
                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Ignore;

                using (StreamWriter sw = new StreamWriter(fileInfo.FullName))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, ExamplesPatterns);
                }
            }
        }

        private Category [] categories;
        private XElement aiml = new XElement("aiml");

        public MaxBrain(Category [] categories)
        {
            this.categories = categories;
        }

        public void save(string filePath)
        {
            var x_settings = new XmlWriterSettings();
            x_settings.NewLineChars = Environment.NewLine;
            x_settings.NewLineHandling = NewLineHandling.None;
            x_settings.CloseOutput = true;
            x_settings.Indent = true;

            foreach (Category category in categories)
            {
                aiml.Add(category.ToXml());
            }

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                using (XmlWriter xw = XmlWriter.Create(fs, x_settings))
                {
                    aiml.WriteTo(xw);
                }
            }
        }

        public void LearnIt()
        {
            
        }

        public string ToXMLString()
        {
            return aiml.ToString();
        }

    }
    public class Category
    {
        public string pattern { get; set; }
        public Response response { get; set; }
        public Category(string pattern, Response response)
        {
            this.pattern = pattern;
            this.response = response;
        }
        public XElement ToXml()
        {
            XElement category = new XElement("category");
            XElement pattern = new XElement("pattern", this.pattern);

            XElement template;

            if (response.GetType() == typeof(TextResponse))
            {
                template = new XElement("template", response.getXString());
            }
            else
            {
                template = new XElement("template", response.getXElement());
            }

            category.Add(pattern);
            category.Add(template);
            return category;
        }
    }
    public class TextResponse : Response
    {
        private string text;
        public TextResponse(string text)
        {
            this.text = text;
        }
        public override XElement getXElement()
        {
            return null;
        }
        public override string getXString()
        {
            return text;
        }
    }

    public class MusicResponse : Response
    {
        private string Uri;
        public MusicResponse(string uri)
        {
            this.Uri = uri;
        }
        public override XElement getXElement()
        {
            XElement element = new XElement("music");
            element.SetAttributeValue("uri", this.Uri);
            return element;
        }
    }

    public class AppResponse : Response
    {
        private string Name;
        private string Id;
        public AppResponse(string name, string id)
        {
            this.Name = name;
            this.Id = id;
        }

        public override XElement getXElement()
        {
            XElement element = new XElement("app");
            element.SetAttributeValue("name", this.Name);
            element.SetAttributeValue("id", this.Id);
            return element;
        }
    }

    public class RandomResponse : Response
    {
        private List<string> randomList;

        public RandomResponse(List<string> randomList)
        {
            this.randomList = randomList;
        }

        public override XElement getXElement()
        {
            XElement element = new XElement("random");
            foreach (string text in randomList)
            {
                XElement item = new XElement("li", text);
                element.Add(item);
            }
            return element;
        }
    }

    public abstract class Response
    {
        public abstract XElement getXElement();
        public virtual string getXString()
        {
            XElement element = getXElement();
            return (element != null) ? element.ToString() : null;
        }
    }
    public class LearnPattern
    {
        public LearnPattern(string pattern)
        {
            this.pattern = pattern;
        }
        public string pattern { get; set; }
    }
}
