using Foxconn.TestUI.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foxconn.TestUI.Editor
{
    public class ProgramManager
    {
        private string _filePath = @$"data\board.json";
        private Board _program = new Board();
        public Board Program
        {
            get => _program;
            set
            {
                _program = value;
            }
        }

        private static ProgramManager _instance;
        public static ProgramManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ProgramManager();
                }
                return _instance;
            }
        }

        public void OpenProgram()
        {
            try
            {
                var lst = new List<string>
                {
                "data",
                "docs",
                "logs",
                "params",
                "temp"
                };
                foreach (var dir in lst)
                {
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }

                // string filename = @"data\board.json";
                if (File.Exists(_filePath))
                {
                    Board data = JsonConvert.DeserializeObject<Board>(File.ReadAllText(_filePath));
                    if (data != null)
                    {
                        _program = data;
                    }
                }
                else
                {
                    _program = new Board();
                    _program.Name = "DEFAULT_PROGRAM";
                    SaveProgram();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void SaveProgram()
        {
            try
            {
                string data = JsonConvert.SerializeObject(_program);
                File.WriteAllText(_filePath, data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
