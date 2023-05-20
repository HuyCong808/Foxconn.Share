
using Foxconn.AutoWeight.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Foxconn.AutoWeight.FoxconnEdit
{
    public class ProgramManager
    {
        private string _filePath = @"data\board.json";
        private Board _program = new Board();
        public Board Program
        {
            get => _program;
            set
            {
                _program = value;
            }
        }

        private static ProgramManager _current;
        public static ProgramManager Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new ProgramManager();
                }
                return _current;
            }
        }

        public void OpenProgram()
        {
            try
            {
                ProjectLayout.Init();

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

