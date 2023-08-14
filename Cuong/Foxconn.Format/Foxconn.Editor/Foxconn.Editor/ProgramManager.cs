using Foxconn.Editor.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace Foxconn.Editor
{
    public class ProgramManager : NotifyProperty
    {
        private ObservableCollection<Board> _programs = new ObservableCollection<Board>();
        private Board _program = new Board();
        private DataBase _database = new DataBase();
        public string _filePath = @"data\board.json";

        public ObservableCollection<Board> Programs
        {
            get => _programs;
            set
            {
                _programs = value;
                NotifyPropertyChanged();
            }
        }

        public Board Program
        {
            get => _program;
            set
            {
                _program = value;
            }
        }

        public DataBase Database
        {
            get => _database;
            set => _database = value;
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
               // ReadDataBase();
                //   _program.LoadProgram();

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


        public void ReadDataBase()
        {
            //  _database.Load();
            try
            {
                string _filepath = @"params\database.json";
                if (File.Exists(_filepath))
                {
                    _database = JsonConvert.DeserializeObject<DataBase>(File.ReadAllText(_filepath));
                }
                else
                {
                    _database = new DataBase();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void SaveDataBase()
        {
            _database.Save();
        }

    }

}

