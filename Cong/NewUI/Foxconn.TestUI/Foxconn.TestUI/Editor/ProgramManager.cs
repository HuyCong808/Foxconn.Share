using Foxconn.TestUI.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Foxconn.TestUI.Editor
{
    public class ProgramManager
    {
        private Board _program = new Board();
        public string _filePath = @"data\board.json";
        private Image _imageBoard { get; set; }
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
                // _program.LoadProgram();
                if (File.Exists(_filePath))
                {
                    Board data = JsonConvert.DeserializeObject<Board>(File.ReadAllText(_filePath));
                    if (data != null)
                    {
                        _program = data;
                    }
                    if (_program.ImageBoard != null)
                    {
                        for (int i = 0; i < _program.ImageBoard.Blocks.Count; i++)
                        {
                            bool loaded = _program.ImageBoard.Blocks[i].Load($"data\\images\\image_{_program.ImageBoard.Blocks[i].Name}.png");
                            if (!loaded)
                            {
                                _program.ImageBoard.Dispose();
                            }
                        }
                    }
                }
                else
                {
                    _program = new Board();
                    _program.Name = "DEFAULT_PROGRAM";
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
                _program.SaveProgram();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
