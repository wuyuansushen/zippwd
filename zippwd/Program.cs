﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace zippwd
{
    public class Program
    {
        public static ZipEntryFactory zipEntryFactory = new ZipEntryFactory();
        static void Main(string[] args)
        {
            string pureName = @"", prefixOfPath = @"";
            DeleteFileAndDirectory(args[0]);
            using (var fs = File.Create(args[0]))
            {
                using (var outStream = new ZipOutputStream(fs))
                {
                    outStream.SetLevel(7);
                    outStream.Password = (args[args.Length - 1]);
                    for (int i = 1; i < args.Length - 1; i++)
                    {
                        pureName = GetPureName(args[i]);
                        prefixOfPath = pureName.Substring(0, pureName.Length -
                            (pureName.Split(Path.DirectorySeparatorChar).Last()).Length);
                        if (prefixOfPath.Length > 0)
                        {
                            Compress(pureName, outStream, prefixOfPath);
                        }
                        else
                        {
                            //No prefix
                            Compress(pureName, outStream);
                        }
                    }
                }
            }
            for (int i = 1; i < args.Length - 1; i++)
            {
                pureName = GetPureName(args[i]);
                DeleteFileAndDirectory(pureName);
            }
        }

        public static string GetPureName(string inName)
        {
            var cleanName = inName.Trim();
            string? tmpName = null;
            if (cleanName.Last() == Path.DirectorySeparatorChar)
            {
                //Clean the last slash(/)
                tmpName = cleanName.Substring(0, cleanName.Length - 1);
            }
            else
            {
                //Don't need to clean
                tmpName = cleanName;
            }
            return tmpName;
        }
        public static void DeleteFileAndDirectory(string fileDirectoryName)
        {
            if (File.Exists(fileDirectoryName) || Directory.Exists(fileDirectoryName))
            {

                var attr = File.GetAttributes(fileDirectoryName);
                if (!attr.HasFlag(FileAttributes.Directory))
                {
                    File.Delete(fileDirectoryName);
                }
                else
                {
                    Directory.Delete(fileDirectoryName, true);
                }
            }
            else
            {
            }
        }
        static void Compress(string path, ZipOutputStream inputStream, string prefixGet = @"")
        {
            var attr = File.GetAttributes(path);
            if (!attr.HasFlag(FileAttributes.Directory))
            {
                CompressFiles(path, inputStream, prefixGet);
            }
            else
            {
                //Get all file paths below this directories.
                var files = Directory.GetFiles(path);
                CompressCurrentDirectory(path, inputStream, prefixGet);

                foreach (var singleFile in files)
                {
                    CompressFiles(singleFile, inputStream, prefixGet);
                }

                //Recursively
                var subFolders = Directory.GetDirectories(path);
                foreach (var dire in subFolders)
                {
                    Compress(dire, inputStream, prefixGet);
                }
            }
        }

        public static void CompressCurrentDirectory(string path, ZipOutputStream inputStream, string prefixAdjustment)
        {
            var EntryName = ZipEntry.CleanName(path);
            ZipEntry? newEntry = null;
            if (prefixAdjustment.Length > 0)
            {
                newEntry = zipEntryFactory.MakeDirectoryEntry(EntryName.Substring(prefixAdjustment.Length - 1), true);
            }
            else
            {
                newEntry = zipEntryFactory.MakeDirectoryEntry(EntryName, true);
            }
            newEntry.IsUnicodeText = true;
            inputStream.PutNextEntry(newEntry);
        }
        public static void CompressFiles(string path, ZipOutputStream inputStream, string prefixAdjustment)
        {
            var EntryName = ZipEntry.CleanName(path);
            ZipEntry newEntry = CreateEntryMore(EntryName, prefixAdjustment);
            newEntry.IsUnicodeText = true;
            inputStream.PutNextEntry(newEntry);
            using (FileStream fileStream = File.OpenRead(path))
            {
                fileStream.CopyTo(inputStream, 2048);
            }
            //Delete immediately;
            DeleteFileAndDirectory(path);
        }

        public static ZipEntry CreateEntryMore(string entryNameString, string prefixName)
        {
            ZipEntry? newEntry = null;
            if (prefixName.Length > 0)
            {
                newEntry = zipEntryFactory.MakeFileEntry(entryNameString, entryNameString.Substring(prefixName.Length - 1), true);
            }
            else
            {
                newEntry = zipEntryFactory.MakeFileEntry(entryNameString, true);
            }
            return newEntry;
        }
    }

}