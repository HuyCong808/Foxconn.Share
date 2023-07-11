using Foxconn.Editor.Configuration;
using Foxconn.Editor.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Foxconn.Editor
{
    public class ProgramManager : NotifyProperty
    {
        private string _filePath = string.Empty;
        private ObservableCollection<Board> _programs = new ObservableCollection<Board>();
        private Board _program = new Board();

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
                NotifyPropertyChanged(nameof(Program));
            }
        }

        public static ProgramManager Current => __current;
        private static ProgramManager __current = new ProgramManager();
        private ProgramManager() { }
        static ProgramManager() { }

        public void Init()
        {
            //_program = new Board();
        }

        public void NewProgram()
        {
            try
            {
                NewProgramDialog dialog = new NewProgramDialog();
                dialog.ShowDialog();
                if (dialog.DialogResult == true)
                {
                    _program?.Dispose();
                    _program = dialog.NewProgram;
                    _filePath = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public void OpenProgram()
        {
            try
            {
                Board program = new Board();
                using (System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog())
                {
                    ofd.InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
                    ofd.Filter = "Jobname File | *.jbn";
                    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        WaitingDialog.DoWork("Loading program...", () =>
                        {
                            Board loaded = program.Load(ofd.FileName);
                            if (loaded != null)
                            {
                                _program?.Dispose();
                                _program = loaded;
                                _filePath = ofd.FileName;
                            }
                            else
                            {
                                MessageBox.Show("Unable to load program, please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public void SaveProgram()
        {
            try
            {
                if (_program != null)
                {
                    if (string.IsNullOrEmpty(_filePath) || !File.Exists(_filePath))
                    {
                        using (System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog())
                        {
                            sfd.InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
                            sfd.FileName = _program.Name;
                            sfd.DefaultExt = "jbn";
                            sfd.Filter = "Jobname File | *.jbn";
                            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                _filePath = sfd.FileName;
                                WaitingDialog.DoWork("Save program...", () =>
                                {
                                    bool saved = _program.SaveProgram(sfd.FileName, false);
                                    if (!saved)
                                        MessageBox.Show("Unable to save program, please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                                });
                            }
                        }
                    }
                    else
                    {
                        WaitingDialog.DoWork("Save program...", () =>
                        {
                            bool saved = _program.SaveProgram(_filePath, false);
                            if (!saved)
                                MessageBox.Show("Unable to save program, please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public void SaveProgramAndImage()
        {
            try
            {
                if (_program != null)
                {
                    if (string.IsNullOrEmpty(_filePath) || !File.Exists(_filePath))
                    {
                        using (System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog())
                        {
                            sfd.InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
                            sfd.FileName = _program.Name;
                            sfd.DefaultExt = "jbn";
                            sfd.Filter = "Jobname File | *.jbn";
                            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                _filePath = sfd.FileName;
                                WaitingDialog.DoWork("Save program and image...", () =>
                                {
                                    bool saved = _program.SaveProgram(_filePath, true);
                                    if (!saved)
                                        MessageBox.Show("Unable to save program and image, please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                                });
                            }
                        }
                    }
                    else
                    {
                        WaitingDialog.DoWork("Save program and image...", () =>
                        {
                            bool saved = _program.SaveProgram(_filePath, true);
                            if (!saved)
                                MessageBox.Show("Unable to save program and image, please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public void SaveAsProgram()
        {
            try
            {
                if (_program != null)
                {
                    using (System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog())
                    {
                        sfd.InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
                        sfd.FileName = _program.Name;
                        sfd.DefaultExt = "jbn";
                        sfd.Filter = "Jobname File | *.jbn";
                        if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            _filePath = sfd.FileName;
                            WaitingDialog.DoWork("Save as program...", () =>
                            {
                                bool saved = _program.SaveProgram(_filePath, false);
                                if (!saved)
                                {
                                    MessageBox.Show("Unable to save as program, please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                                }
                                else
                                {
                                    Board loaded = _program.Load(sfd.FileName);
                                    if (loaded != null)
                                    {
                                        _program?.Dispose();
                                        _program = loaded;
                                    }
                                    else
                                    {
                                        MessageBox.Show("Unable to save as program, please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                                    }
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public void SaveAsProgramAndImage()
        {
            try
            {
                using (System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog())
                {
                    sfd.InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
                    sfd.FileName = _program.Name;
                    sfd.DefaultExt = "jbn";
                    sfd.Filter = "Jobname File | *.jbn";
                    if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        _filePath = sfd.FileName;
                        WaitingDialog.DoWork("Save as program and image...", () =>
                        {
                            bool saved = _program.SaveProgram(_filePath, true);
                            if (!saved)
                            {
                                MessageBox.Show("Unable to save as program and image, please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                            }
                            else
                            {
                                Board loaded = _program.Load(sfd.FileName);
                                if (loaded != null)
                                {
                                    _program?.Dispose();
                                    _program = loaded;
                                }
                                else
                                {
                                    MessageBox.Show("Unable to save as program and image, please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                                }
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
    }
}
