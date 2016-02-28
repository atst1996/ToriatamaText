﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ToriatamaText.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var extractor = new Extractor();
            var tests = LoadTests();

            Console.WriteLine("=========================");
            Console.WriteLine("Mentions");
            Console.WriteLine("=========================");
            foreach (var test in tests.Mentions)
            {
                Console.WriteLine(test.Description);
                var result = extractor.ExtractMentionedScreenNames(test.Text)
                    .ConvertAll(x => test.Text.Substring(x.StartIndex + 1, x.Length - 1));
                if (!result.SequenceEqual(test.Expected))
                {
                    Debugger.Break();
                }
            }

            Console.WriteLine();
            Console.WriteLine("=========================");
            Console.WriteLine("MentionsWithIndices");
            Console.WriteLine("=========================");
            foreach (var test in tests.MentionsWithIndices)
            {
                Console.WriteLine(test.Description);
                var result = extractor.ExtractMentionedScreenNames(test.Text)
                    .ConvertAll(x => new MentionsWithIndicesExpected
                    {
                        ScreenName = test.Text.Substring(x.StartIndex + 1, x.Length - 1),
                        Indices = new[] { x.StartIndex, x.StartIndex + x.Length }
                    });
                if (!result.SequenceEqual(test.Expected))
                {
                    Debugger.Break();
                }
            }

            Console.WriteLine();
            Console.WriteLine("=========================");
            Console.WriteLine("MentionsOrListsWithIndices");
            Console.WriteLine("=========================");
            foreach (var test in tests.MentionsOrListsWithIndices)
            {
                Console.WriteLine(test.Description);
                var result = extractor.ExtractMentionsOrLists(test.Text)
                    .ConvertAll(x =>
                    {
                        var s = test.Text.Substring(x.StartIndex + 1, x.Length - 1).Split(new[] { '/' }, 2, StringSplitOptions.RemoveEmptyEntries);
                        return new MentionsOrListsWithIndicesExpected
                        {
                            ScreenName = s[0],
                            ListSlug = s.Length == 2 ? "/" + s[1] : "",
                            Indices = new[] { x.StartIndex, x.StartIndex + x.Length }
                        };
                    });
                if (!result.SequenceEqual(test.Expected))
                {
                    Debugger.Break();
                }
            }

            Console.WriteLine();
            Console.WriteLine("=========================");
            Console.WriteLine("Urls");
            Console.WriteLine("=========================");
            foreach (var test in tests.Urls)
            {
                Console.WriteLine(test.Description);
                var result = extractor.ExtractUrls(test.Text)
                    .ConvertAll(x => test.Text.Substring(x.StartIndex, x.Length));
                if (!result.SequenceEqual(test.Expected))
                {
                    Debugger.Break();
                }
            }

            Console.WriteLine();
            Console.WriteLine("=========================");
            Console.WriteLine("UrlsWithIndices");
            Console.WriteLine("=========================");
            foreach (var test in tests.UrlsWithIndices)
            {
                Console.WriteLine(test.Description);
                var result = extractor.ExtractUrls(test.Text)
                    .ConvertAll(x => new UrlsWithIndicesExpected
                    {
                        Url = test.Text.Substring(x.StartIndex, x.Length),
                        Indices = new[] { x.StartIndex, x.StartIndex + x.Length }
                    });
                if (!result.SequenceEqual(test.Expected))
                {
                    Debugger.Break();
                }
            }

            Console.WriteLine();
            Console.WriteLine("End");
            Console.ReadLine();
        }

        private const string testFile = "extract.yml";

        static void DownloadTests()
        {
            Console.WriteLine("Downloading extract.yml");
            new WebClient().DownloadFile(
                "https://raw.githubusercontent.com/twitter/twitter-text/master/conformance/extract.yml",
                testFile);
        }

        static ExtractorTests LoadTests()
        {
            if (!File.Exists(testFile))
                DownloadTests();

            ExtractYaml testYaml;

            using (var sr = new StreamReader(testFile))
            {
                var deserializer = new Deserializer(namingConvention: new UnderscoredNamingConvention(), ignoreUnmatched: true);
                testYaml = deserializer.Deserialize<ExtractYaml>(sr);
            }

            return testYaml.Tests;
        }
    }
}
