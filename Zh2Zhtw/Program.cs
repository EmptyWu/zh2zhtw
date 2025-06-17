using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Linq;
using OpenCCNET;

class Program
{
    private static Dictionary<string, string> customMappings;
    private static List<string> supportedExtensions;

    static void Main(string[] args)
    {
        args =new string[] { "D:\\98.SampleCode" };
        if (args.Length == 0)
        {
            Console.WriteLine("請提供資料夾路徑作為參數");
            return;
        }

        string folderPath = args[0];
        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine("指定的資料夾不存在");
            return;
        }

        // 載入設定
        LoadConfig();

        // 初始化 OpenCCNET
        ZhConverter.Initialize();

        // 處理檔案
        ProcessFiles(folderPath);
        Console.ReadLine();
    }

    private static void LoadConfig()
    {
        try
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            string jsonString = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<Config>(jsonString);
            
            customMappings = config.CustomMappings;
            supportedExtensions = config.FileExtensions;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"載入設定檔時發生錯誤: {ex.Message}");
            //Environment.Exit(1);
        }
    }

    private static void ProcessFiles(string folderPath)
    {
        var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
            .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLower()));

        int totalFiles = files.Count();
        int processedFiles = 0;
        int successCount = 0;
        int errorCount = 0;

        foreach (var file in files)
        {
            try
            {
                Console.WriteLine($"處理檔案: {file}");
                string content = File.ReadAllText(file, Encoding.UTF8);

                // 先進行自定義轉換
                foreach (var mapping in customMappings)
                {
                    content = content.Replace(mapping.Key, mapping.Value);
                }

                // 使用 OpenCCNET 進行簡體轉繁體
                string convertedContent = ZhConverter.HansToHant(content);

                // 寫回檔案
                File.WriteAllText(file, convertedContent, Encoding.UTF8);
                successCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"處理檔案 {file} 時發生錯誤: {ex.Message}");
                errorCount++;
            }

            processedFiles++;
            Console.WriteLine($"進度: {processedFiles}/{totalFiles}");
        }

        Console.WriteLine("\n處理完成！");
        Console.WriteLine($"成功: {successCount} 個檔案");
        Console.WriteLine($"失敗: {errorCount} 個檔案");
    }
}

class Config
{
    public Dictionary<string, string> CustomMappings { get; set; }
    public List<string> FileExtensions { get; set; }
}
