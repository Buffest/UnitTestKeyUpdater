using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.Dynamics.Jiantzha
{
    class TestKeyUpdater
    {
        static void Main(string[] args)
        {
            foreach(string path in args)
            {
                if (Directory.Exists(path))
                {
                    ProcessDirectory(path);
                }
                else if (File.Exists(path))
                {
                    ProcessFile(path);
                }
                else
                {
                    Console.WriteLine("{0} is not a valid directory or file.", path);
                }
            }
        }

        private static void ProcessDirectory(string dirPath)
        {
            string[] tsFiles = Directory.GetFiles(dirPath, "*UnitTests.ts", SearchOption.AllDirectories);
            foreach (string file in tsFiles)
            {
                ProcessFile(file);
            }
        }

        private static void ProcessFile(string filePath)
        {
            if (!isValidFileType(filePath))
            {
                Console.WriteLine("{0} is not a supported file type.", filePath);
                return;
            }

            Console.WriteLine("Processing {0}...", filePath);
            bool fileIsReadOnly = isReadOnly(filePath);
            if (fileIsReadOnly)
            {
                removeReadOnly(filePath);
            }
            
            string[] originalContent = File.ReadAllLines(filePath);
            string[] updatedContent = UpdateTestKeys(originalContent);

            string fullText = File.ReadAllText(filePath);

            using (StreamWriter writer = new StreamWriter(filePath)) {
                for (int i = 0; i < updatedContent.Length; i++) {
                    if (i == updatedContent.Length - 1 && !fullText.EndsWith(Environment.NewLine)) {
                        writer.Write(updatedContent[i]);
                    }
                    else {
                        writer.WriteLine(updatedContent[i]);
                    }
                }
            }

            if (fileIsReadOnly)
            {
                addReadOnly(filePath);
            }
        }

        private static string[] UpdateTestKeys(string[] originalContent)
        {
            string guidPattern = @"TestKey [0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}";
            Regex guidRegex = new Regex(guidPattern, RegexOptions.IgnoreCase);

            List<string> updatedContent = new List<string>();
            foreach (string line in originalContent)
            {
                Match guidMatch = guidRegex.Match(line);
                if (guidMatch.Success)
                {
                    string originalGuid = guidMatch.Value;
                    string updatedGuid = addGuidLastFourDigitsByOne(originalGuid);
                    string updatedLine = guidRegex.Replace(line, updatedGuid);
                    updatedContent.Add(updatedLine);
                }
                else
                {
                    updatedContent.Add(line);
                }
            }

            return updatedContent.ToArray();
        }

        private static bool isValidFileType(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            return extension == ".ts";
        }

        private static bool isReadOnly(string filePath)
        {
            FileAttributes attributes = File.GetAttributes(filePath);
            return (attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
        }

        private static void removeReadOnly(string filePath)
        {
            FileAttributes attributes = File.GetAttributes(filePath);
            attributes = attributes & ~FileAttributes.ReadOnly;
            File.SetAttributes(filePath, attributes);
        }

        private static void addReadOnly(string filePath)
        {
            FileAttributes attributes = File.GetAttributes(filePath);
            attributes = attributes | FileAttributes.ReadOnly;
            File.SetAttributes(filePath, attributes);
        }

        private static string addGuidLastFourDigitsByOne(string guid)
        {
            char[] hex = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};
            char[] guidChars = guid.ToCharArray();
            for (int i = guid.Length - 4; i < guid.Length; i++)
            {
                int hexIndex = Array.IndexOf(hex, guidChars[i]);
                guidChars[i] = hex[(hexIndex + 1) % hex.Length];
            }

            return new string(guidChars);
        }
    }
}
