/*
using Delimon.Win32.IO.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Delimon.Win32.IO
{
    public class IOBackGroundWorker
    {
        private IOBackGroundWorker.Completed _completedEvent;
        private IOBackGroundWorker.ProgressChanged _progressChangedEvent;
        private IOBackGroundWorkerArguments _args;
        private BackgroundWorker _bg;
        private frmIOWorker _frm;
        private string _ID;

        public string ID
        {
            get
            {
                return this._ID;
            }
        }

        public bool IsBusy
        {
            get
            {
                if (this._bg == null)
                    return false;
                else
                    return this._bg.IsBusy;
            }
        }

        public bool CancellationPending
        {
            get
            {
                if (this._bg == null)
                    return false;
                else
                    return this._bg.CancellationPending;
            }
        }

        public event IOBackGroundWorker.Completed CompletedEvent
        {
            add
            {
                IOBackGroundWorker.Completed completed = this._completedEvent;
                IOBackGroundWorker.Completed comparand;
                do
                {
                    comparand = completed;
                    completed = Interlocked.CompareExchange<IOBackGroundWorker.Completed>(ref this._completedEvent, comparand + value, comparand);
                }
                while (completed != comparand);
            }
            remove
            {
                IOBackGroundWorker.Completed completed = this._completedEvent;
                IOBackGroundWorker.Completed comparand;
                do
                {
                    comparand = completed;
                    completed = Interlocked.CompareExchange<IOBackGroundWorker.Completed>(ref this._completedEvent, comparand - value, comparand);
                }
                while (completed != comparand);
            }
        }

        public event IOBackGroundWorker.ProgressChanged ProgressChangedEvent
        {
            add
            {
                IOBackGroundWorker.ProgressChanged progressChanged = this._progressChangedEvent;
                IOBackGroundWorker.ProgressChanged comparand;
                do
                {
                    comparand = progressChanged;
                    progressChanged = Interlocked.CompareExchange<IOBackGroundWorker.ProgressChanged>(ref this._progressChangedEvent, comparand + value, comparand);
                }
                while (progressChanged != comparand);
            }
            remove
            {
                IOBackGroundWorker.ProgressChanged progressChanged = this._progressChangedEvent;
                IOBackGroundWorker.ProgressChanged comparand;
                do
                {
                    comparand = progressChanged;
                    progressChanged = Interlocked.CompareExchange<IOBackGroundWorker.ProgressChanged>(ref this._progressChangedEvent, comparand - value, comparand);
                }
                while (progressChanged != comparand);
            }
        }

        public IOBackGroundWorker(IOBackGroundWorkerArguments args)
        {
            this.InitializeIOBackGroundWorker(args, args.Destination);
        }

        public IOBackGroundWorker(IOBackGroundWorkerArguments args, string id)
        {
            this.InitializeIOBackGroundWorker(args, id);
        }

        private void InitializeIOBackGroundWorker(IOBackGroundWorkerArguments args, string id)
        {
            this._ID = id;
            if (args.Type == IOBackGroundWorkerType.NoTypeSpecified)
                throw new Exception("IOBackGroundWorkerType must be set in IOBackGroundWorkerArguments when you instantiate a IOBackGroundWorker");
            if (args.SourceFiles.Count == 0 && args.SourceFolders.Count == 0)
                throw new Exception("SourceFiles and/or SourceFolders must be set in IOBackGroundWorkerArguments when you instantiate a IOBackGroundWorker");
            this._bg = new BackgroundWorker();
            this._bg.WorkerReportsProgress = true;
            this._bg.WorkerSupportsCancellation = true;
            this._args = args;
            switch (args.Type)
            {
                case IOBackGroundWorkerType.Copy:
                    if (args.Destination == null)
                        throw new Exception("Destination must be set in IOBackGroundWorkerArguments when you instantiate a IOBackGroundWorker with type IOBackGroundWorkerType.Copy");
                    else
                        break;
                case IOBackGroundWorkerType.Move:
                    if (args.Destination == null)
                        throw new Exception("Destination must be set in IOBackGroundWorkerArguments when you instantiate a IOBackGroundWorker with type IOBackGroundWorkerType.Move");
                    else
                        break;
            }
            if (args.ShowUI)
                return;
            args.AllwaysSkipOnErrorFiles = true;
            args.AllwaysSkipOnErrorFolders = true;
            switch (args.Type)
            {
                case IOBackGroundWorkerType.Copy:
                    if (!args.AllwaysOverwriteFiles & !args.NeverOverwriteFiles)
                        throw new Exception("You must set IOBackGroundWorkerArguments : AllwaysOverwriteFiles or NeverOverwriteFiles when ShowUI = false");
                    if (!args.AllwaysOverwirteReadOnlyFiles & !args.NeverOverwriteReadOnlyFiles)
                        throw new Exception("You must set IOBackGroundWorkerArguments : AllwaysOverwirteReadOnlyFiles or NeverOverwriteReadOnlyFiles when ShowUI = false");
                    if (!(!args.AllwaysOverwriteFolders & !args.NeverOverwriteFolders))
                        break;
                    else
                        throw new Exception("You must set IOBackGroundWorkerArguments : AllwaysOverwriteFolders or NeverOverwriteFolders when ShowUI = false");
                case IOBackGroundWorkerType.Move:
                    if (!args.AllwaysOverwriteFiles & !args.NeverOverwriteFiles)
                        throw new Exception("You must set IOBackGroundWorkerArguments : AllwaysOverwriteFiles or NeverOverwriteFiles when ShowUI = false");
                    if (!args.AllwaysOverwirteReadOnlyFiles & !args.NeverOverwriteReadOnlyFiles)
                        throw new Exception("You must set IOBackGroundWorkerArguments : AllwaysOverwirteReadOnlyFiles or NeverOverwriteReadOnlyFiles when ShowUI = false");
                    if (!args.AllwaysOverwriteFolders & !args.NeverOverwriteFolders)
                        throw new Exception("You must set IOBackGroundWorkerArguments : AllwaysOverwriteFolders or NeverOverwriteFolders when ShowUI = false");
                    if (!(!args.AllwaysDeleteReadOnlyFiles & !args.NeverDeleteReadOnlyFiles))
                        break;
                    else
                        throw new Exception("You must set IOBackGroundWorkerArguments : AllwaysDeleteReadOnlyFiles or NeverDeleteReadOnlyFiles when ShowUI = false");
                case IOBackGroundWorkerType.Delete:
                    if (!(!args.AllwaysDeleteReadOnlyFiles & !args.NeverDeleteReadOnlyFiles))
                        break;
                    else
                        throw new Exception("You must set IOBackGroundWorkerArguments : AllwaysDeleteReadOnlyFiles or NeverDeleteReadOnlyFiles when ShowUI = false");
            }
        }

        private void ThrowNotImplementedError()
        {
            throw new Exception("This functionality is not yet implemented!");
        }

        public void start()
        {
            if (this._bg.IsBusy)
                return;
            this._bg.DoWork += new DoWorkEventHandler(this._bg_DoWork);
            this._bg.ProgressChanged += new ProgressChangedEventHandler(this._bg_ProgressChanged);
            this._bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this._bg_RunWorkerCompleted);
            if (this._args.ShowUI)
            {
                this._frm = new frmIOWorker();
                ((Control)this._frm).Show();
                this._frm.prgProgress.Value = 100;
                this._frm.prgProgress.Style = ProgressBarStyle.Blocks;
                this._frm.btnCancel.Click += new EventHandler(this.btnCancel_Click);
                this._frm.FormClosing += new FormClosingEventHandler(this.frm_FormClosing);
            }
            this._bg.RunWorkerAsync((object)this._args);
        }

        private void frm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!this._bg.IsBusy)
                return;
            e.Cancel = true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (this._bg.IsBusy)
            {
                this._frm.btnCancel.Text = "Cancelling ...";
                this._frm.btnCancel.Enabled = false;
                this.Stop();
            }
            else
                this._frm.Close();
        }

        public void Stop()
        {
            if (!this._bg.IsBusy)
                return;
            this._bg.CancelAsync();
        }

        private void _bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IOBackGroundWorkerProgress progress = (IOBackGroundWorkerProgress)null;
            if (this._args.ShowUI)
            {
                try
                {
                    progress = e.Result as IOBackGroundWorkerProgress;
                    progress.CurrentOperation = "Finished";
                }
                catch
                {
                }
                this._frm.prgProgress.Value = 100;
                if (e.Error != null)
                {
                    this._frm.prgProgress.Style = ProgressBarStyle.Marquee;
                    this._frm.btnCancel.Text = "Close";
                    this._frm.lblOperation.Text = e.Error.Message;
                    this._frm.lblOperation.ForeColor = Color.Red;
                }
                else
                {
                    this._frm.prgProgress.Style = ProgressBarStyle.Blocks;
                    this._frm.lblOperation.Text = "Finished";
                    this._frm.lblFolder.Text = "Done";
                    this._frm.lblFile.Text = "Done";
                    this._frm.Close();
                    this._frm.Dispose();
                }
            }
            try
            {
                this.CompletedEvent(progress, (object)this, e);
            }
            catch (Exception ex)
            {
            }
        }

        private void _bg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            IOBackGroundWorkerProgress progress = e.UserState as IOBackGroundWorkerProgress;
            if (this._args.ShowUI)
            {
                this._frm.lblOperation.Text = progress.CurrentOperation;
                this._frm.lblFolder.Text = progress.CurrentFolderDisplay;
                this._frm.lblFile.Text = progress.CurrentFile;
                if (!progress.Calculating)
                    this._frm.prgProgress.Value = e.ProgressPercentage;
            }
            try
            {
                this.ProgressChangedEvent(progress, (object)this);
            }
            catch (Exception ex)
            {
            }
        }

        private void _bg_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            IOBackGroundWorkerProgress prg = new IOBackGroundWorkerProgress();
            IOBackGroundWorkerArguments args = (IOBackGroundWorkerArguments)e.Argument;
            prg.CurrentOperation = "Calculating Folders and Files ...";
            prg.Calculating = true;
            prg.CurrentFolder = "";
            prg.CurrentFile = "";
            prg.CurrentFolderDisplay = "";
            worker.ReportProgress(100, (object)prg);
            this.EnumerateSources(worker, prg, args, e);
            prg.Calculating = false;
            switch (args.Type)
            {
                case IOBackGroundWorkerType.Copy:
                    prg.CurrentOperation = "Copying Files and Folders ...";
                    foreach (string str in args.SourceFolders)
                        this.CopyMoveDirectory(str, Delimon.Win32.IO.Path.Combine(args.Destination, Delimon.Win32.IO.Path.GetFileName(str)), worker, prg, args, e);
                    using (List<string>.Enumerator enumerator = args.SourceFiles.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            string current = enumerator.Current;
                            this.CopyMoveFile(current, Delimon.Win32.IO.Path.Combine(args.Destination, Delimon.Win32.IO.Path.GetFileName(current)), prg, args);
                        }
                        break;
                    }
                case IOBackGroundWorkerType.Move:
                    prg.CurrentOperation = "Moving Files and Folders ...";
                    foreach (string str in args.SourceFolders)
                    {
                        if (Delimon.Win32.IO.Path.GetPathRoot(str).Equals(Delimon.Win32.IO.Path.GetPathRoot(args.Destination)))
                        {
                            if (Delimon.Win32.IO.Directory.Exists(Delimon.Win32.IO.Path.Combine(args.Destination, Delimon.Win32.IO.Path.GetFileName(str))))
                            {
                                this.CopyMoveDirectory(str, Delimon.Win32.IO.Path.Combine(args.Destination, Delimon.Win32.IO.Path.GetFileName(str)), worker, prg, args, e);
                            }
                            else
                            {
                                Delimon.Win32.IO.Directory.Move(str, Delimon.Win32.IO.Path.Combine(args.Destination, Delimon.Win32.IO.Path.GetFileName(str)));
                                ++prg.FoldersProcessed;
                                ++prg.FoldersMoved;
                                prg.CurrentFolder = str;
                                this.ReportProgress(worker, prg);
                            }
                        }
                        else
                            this.CopyMoveDirectory(str, Delimon.Win32.IO.Path.Combine(args.Destination, Delimon.Win32.IO.Path.GetFileName(str)), worker, prg, args, e);
                    }
                    using (List<string>.Enumerator enumerator = args.SourceFiles.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            string current = enumerator.Current;
                            if (Delimon.Win32.IO.Path.GetPathRoot(current).Equals(Delimon.Win32.IO.Path.GetPathRoot(args.Destination)))
                            {
                                if (Delimon.Win32.IO.File.Exists(Delimon.Win32.IO.Path.Combine(args.Destination, Delimon.Win32.IO.Path.GetFileName(current))))
                                {
                                    this.CopyMoveFile(current, Delimon.Win32.IO.Path.Combine(args.Destination, Delimon.Win32.IO.Path.GetFileName(current)), prg, args);
                                }
                                else
                                {
                                    Delimon.Win32.IO.File.Move(current, Delimon.Win32.IO.Path.Combine(args.Destination, Delimon.Win32.IO.Path.GetFileName(current)));
                                    ++prg.FilesProcessed;
                                    ++prg.FilesMoved;
                                    prg.CurrentFolder = Delimon.Win32.IO.Path.GetDirectoryName(current);
                                    prg.CurrentFile = current;
                                    this.ReportProgress(worker, prg);
                                }
                            }
                            else
                                this.CopyMoveFile(current, Delimon.Win32.IO.Path.Combine(args.Destination, Delimon.Win32.IO.Path.GetFileName(current)), prg, args);
                        }
                        break;
                    }
                case IOBackGroundWorkerType.Delete:
                    prg.CurrentOperation = "Deleting Files and Folders ...";
                    foreach (string folder in args.SourceFolders)
                        this.DeleteDirectory(folder, prg, args, worker, e);
                    using (List<string>.Enumerator enumerator = args.SourceFiles.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                            this.DeleteFile(enumerator.Current, prg, args);
                        break;
                    }
            }
            Thread.Sleep(1000);
            e.Result = (object)prg;
        }

        public void EnumerateSources(BackgroundWorker worker, IOBackGroundWorkerProgress prg, IOBackGroundWorkerArguments args, DoWorkEventArgs e)
        {
            foreach (string str in args.SourceFolders)
            {
                if (args.Type == IOBackGroundWorkerType.Move && Delimon.Win32.IO.Path.GetPathRoot(str).Equals(Delimon.Win32.IO.Path.GetPathRoot(args.Destination)))
                {
                    if (Delimon.Win32.IO.Directory.Exists(Delimon.Win32.IO.Path.Combine(args.Destination, Delimon.Win32.IO.Path.GetFileName(str))))
                        this.EnumerateDirectory(str, worker, prg, args, e);
                    else
                        ++prg.FoldersFound;
                }
                else
                    this.EnumerateDirectory(str, worker, prg, args, e);
            }
            foreach (string str in args.SourceFiles)
            {
                ++prg.FilesFound;
                if (args.Type == IOBackGroundWorkerType.Move)
                    ++prg.FilesFound;
            }
        }

        public void EnumerateDirectory(string source, BackgroundWorker worker, IOBackGroundWorkerProgress prg, IOBackGroundWorkerArguments args, DoWorkEventArgs e)
        {
            ++prg.FoldersFound;
            foreach (string str in Delimon.Win32.IO.Directory.GetFiles(source))
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                else
                    ++prg.FilesFound;
            }
            if (!args.Recursive)
                return;
            foreach (string source1 in Delimon.Win32.IO.Directory.GetDirectories(source))
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                else
                    this.EnumerateDirectory(source1, worker, prg, args, e);
            }
        }

        private void ReportProgress(BackgroundWorker worker, IOBackGroundWorkerProgress prg)
        {
            if (prg.FilesFound + prg.FoldersFound == 0L)
            {
                prg.ProgressPercentage = 0;
                worker.ReportProgress(0, (object)prg);
            }
            else
            {
                int percentProgress = Convert.ToInt32(Decimal.Floor(Decimal.Multiply(Decimal.Divide((Decimal)(prg.FilesProcessed + prg.FoldersProcessed), (Decimal)(prg.FilesFound + prg.FoldersFound)), new Decimal(100))));
                prg.CurrentFolderDisplay = prg.CurrentFolder.Length <= 50 ? prg.CurrentFolder : Delimon.Win32.IO.Path.GetPathRoot(prg.CurrentFolder) + "..." + prg.CurrentFolder.Substring(prg.CurrentFolder.Length - 49, 49);
                if (percentProgress > 100)
                    percentProgress = 100;
                prg.ProgressPercentage = percentProgress;
                worker.ReportProgress(percentProgress, (object)prg);
            }
        }

        public void CopyMoveDirectory(string source, string destination, BackgroundWorker worker, IOBackGroundWorkerProgress prg, IOBackGroundWorkerArguments args, DoWorkEventArgs e)
        {
            Trace.TraceInformation(source);
            Trace.TraceInformation(destination);
            prg.CurrentFolder = source;
            ++prg.FoldersProcessed;
            this.ReportProgress(worker, prg);
            if (!Delimon.Win32.IO.Directory.Exists(destination))
            {
                if (!this.CreateDirectory(destination, prg, args))
                    return;
            }
            else if (!args.AllwaysOverwriteFolders)
            {
                if (args.NeverOverwriteFolders)
                {
                    --prg.FoldersCopied;
                    ++prg.FoldersSkipped;
                    return;
                }
                else
                {
                    frmIOMessage frmIoMessage = new frmIOMessage(IOMessageType.Warning, IOMessageIcon.Folder);
                    frmIoMessage.lblMessage.Text = "Folder : " + Environment.NewLine + destination + Environment.NewLine + "Allready exists do you want to Continue ?";
                    if (frmIoMessage.ShowDialog() != DialogResult.Yes)
                    {
                        if (frmIoMessage.chkAllways.Checked)
                            args.NeverOverwriteFolders = frmIoMessage.chkAllways.Checked;
                        --prg.FoldersCopied;
                        ++prg.FoldersSkipped;
                        return;
                    }
                    else if (frmIoMessage.chkAllways.Checked)
                        args.AllwaysOverwriteFolders = frmIoMessage.chkAllways.Checked;
                }
            }
            foreach (string str1 in Delimon.Win32.IO.Directory.GetFiles(source))
            {
                string str2 = Delimon.Win32.IO.Path.Combine(destination, Delimon.Win32.IO.Path.GetFileName(str1));
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    prg.CurrentFile = Delimon.Win32.IO.Path.GetFileName(str1);
                    if ((Delimon.Win32.IO.File.GetAttributes(str2) & Delimon.Win32.IO.FileAttributes.ReadOnly) == Delimon.Win32.IO.FileAttributes.ReadOnly)
                    {
                        if (!args.AllwaysOverwirteReadOnlyFiles)
                        {
                            if (args.NeverOverwriteReadOnlyFiles)
                            {
                                ++prg.FilesSkipped;
                            }
                            else
                            {
                                frmIOMessage frmIoMessage = new frmIOMessage(IOMessageType.Warning, IOMessageIcon.File);
                                frmIoMessage.lblMessage.Text = "File : " + Environment.NewLine + str2 + Environment.NewLine + "Is ReadOnly!" + Environment.NewLine + "Do you want to Overwrite this File ?";
                                if (frmIoMessage.ShowDialog() == DialogResult.Yes)
                                {
                                    Delimon.Win32.IO.File.SetAttributes(str2, Delimon.Win32.IO.FileAttributes.Archive | Delimon.Win32.IO.FileAttributes.Normal);
                                    this.CopyMoveFile(str1, str2, prg, args);
                                    if (frmIoMessage.chkAllways.Checked)
                                        args.AllwaysOverwirteReadOnlyFiles = frmIoMessage.chkAllways.Checked;
                                }
                                else
                                {
                                    if (frmIoMessage.chkAllways.Checked)
                                        args.NeverOverwriteReadOnlyFiles = frmIoMessage.chkAllways.Checked;
                                    ++prg.FilesSkipped;
                                }
                            }
                        }
                        else
                        {
                            Delimon.Win32.IO.File.SetAttributes(str2, Delimon.Win32.IO.FileAttributes.Archive | Delimon.Win32.IO.FileAttributes.Normal);
                            this.CopyMoveFile(str1, str2, prg, args);
                        }
                    }
                    else if (Delimon.Win32.IO.File.Exists(str2))
                    {
                        if (!args.AllwaysOverwriteFiles)
                        {
                            if (args.NeverOverwriteFiles)
                            {
                                ++prg.FilesSkipped;
                            }
                            else
                            {
                                frmIOMessage frmIoMessage = new frmIOMessage(IOMessageType.Question, IOMessageIcon.File);
                                frmIoMessage.lblMessage.Text = "File : " + Environment.NewLine + str2 + Environment.NewLine + "Allready Exists!" + Environment.NewLine + "Do you want to Overwrite this File ?";
                                if (frmIoMessage.ShowDialog() == DialogResult.Yes)
                                {
                                    if (frmIoMessage.chkAllways.Checked)
                                        args.AllwaysOverwriteFiles = frmIoMessage.chkAllways.Checked;
                                    this.CopyMoveFile(str1, str2, prg, args);
                                }
                                else
                                {
                                    if (frmIoMessage.chkAllways.Checked)
                                        args.NeverOverwriteFiles = frmIoMessage.chkAllways.Checked;
                                    ++prg.FilesSkipped;
                                }
                            }
                        }
                        else
                            this.CopyMoveFile(str1, str2, prg, args);
                    }
                    else
                        this.CopyMoveFile(str1, str2, prg, args);
                    ++prg.FilesProcessed;
                    this.ReportProgress(worker, prg);
                }
            }
            if (args.Recursive)
            {
                foreach (string str in Delimon.Win32.IO.Directory.GetDirectories(source))
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                        this.CopyMoveDirectory(str, Delimon.Win32.IO.Path.Combine(destination, Delimon.Win32.IO.Path.GetFileName(str)), worker, prg, args, e);
                }
            }
            if (args.Type != IOBackGroundWorkerType.Move)
                return;
            this.DeleteDirectory(source, prg, args, worker, e);
        }

        private void CopyMoveFile(string source, string destination, IOBackGroundWorkerProgress prg, IOBackGroundWorkerArguments args)
        {
            try
            {
                Delimon.Win32.IO.File.Copy(source, destination, true);
                ++prg.FilesCopied;
                if (args.Type != IOBackGroundWorkerType.Move)
                    return;
                this.DeleteFile(source, prg, args);
                ++prg.FilesMoved;
            }
            catch (Exception ex)
            {
                IOException ioException = new IOException("Source : " + source + Environment.NewLine + "Destination : " + destination + Environment.NewLine + ex.Message, ex);
                prg.Errors.Add((Exception)ioException);
                ++prg.FilesError;
                ++prg.FilesSkipped;
                if (args.AllwaysSkipOnErrorFiles)
                    return;
                frmIOMessage frmIoMessage = new frmIOMessage(IOMessageType.Error, IOMessageIcon.File);
                frmIoMessage.lblMessage.Text = "Copy File Error : " + Environment.NewLine + "Source : " + source + Environment.NewLine + "Destination : " + destination + Environment.NewLine + "Error : " + ex.Message + Environment.NewLine + "Do you want Skip this File or Cancel the operation ?";
                frmIoMessage.btnYes.Text = "Skip";
                frmIoMessage.btnNo.Text = "Cancel";
                if (frmIoMessage.ShowDialog() != DialogResult.Yes)
                    throw new IOException(ex.Message + " : " + destination, ex);
                args.AllwaysSkipOnErrorFiles = frmIoMessage.chkAllways.Checked;
            }
        }

        private void DeleteFile(string file, IOBackGroundWorkerProgress prg, IOBackGroundWorkerArguments args)
        {
            try
            {
                if ((Delimon.Win32.IO.File.GetAttributes(file) & Delimon.Win32.IO.FileAttributes.ReadOnly) == Delimon.Win32.IO.FileAttributes.ReadOnly)
                {
                    if (!args.AllwaysDeleteReadOnlyFiles)
                    {
                        if (args.NeverDeleteReadOnlyFiles)
                        {
                            ++prg.FilesSkipped;
                        }
                        else
                        {
                            frmIOMessage frmIoMessage = new frmIOMessage(IOMessageType.Warning, IOMessageIcon.File);
                            frmIoMessage.lblMessage.Text = "File : " + Environment.NewLine + file + Environment.NewLine + "Is ReadOnly!" + Environment.NewLine + "Do you want to Delete this File ?";
                            if (frmIoMessage.ShowDialog() == DialogResult.Yes)
                            {
                                Delimon.Win32.IO.File.SetAttributes(file, Delimon.Win32.IO.FileAttributes.Archive | Delimon.Win32.IO.FileAttributes.Normal);
                                Delimon.Win32.IO.File.Delete(file);
                                ++prg.FilesDeleted;
                                if (!frmIoMessage.chkAllways.Checked)
                                    return;
                                args.AllwaysDeleteReadOnlyFiles = frmIoMessage.chkAllways.Checked;
                            }
                            else
                            {
                                if (frmIoMessage.chkAllways.Checked)
                                    args.NeverDeleteReadOnlyFiles = frmIoMessage.chkAllways.Checked;
                                ++prg.FilesSkipped;
                            }
                        }
                    }
                    else
                    {
                        Delimon.Win32.IO.File.SetAttributes(file, Delimon.Win32.IO.FileAttributes.Archive | Delimon.Win32.IO.FileAttributes.Normal);
                        Delimon.Win32.IO.File.Delete(file);
                        ++prg.FilesDeleted;
                    }
                }
                else
                {
                    Delimon.Win32.IO.File.Delete(file);
                    ++prg.FilesDeleted;
                }
            }
            catch (Exception ex)
            {
                IOException ioException = new IOException("File : " + file + Environment.NewLine + ex.Message, ex);
                prg.Errors.Add((Exception)ioException);
                ++prg.FilesProcessed;
                ++prg.FilesError;
                ++prg.FilesSkipped;
                --prg.FilesCopied;
                if (args.AllwaysSkipOnErrorFiles)
                    return;
                frmIOMessage frmIoMessage = new frmIOMessage(IOMessageType.Error, IOMessageIcon.File);
                frmIoMessage.lblMessage.Text = "Delete File Error : " + Environment.NewLine + "File : " + file + Environment.NewLine + "Error : " + ex.Message + Environment.NewLine + "Do you want Skip this File or Cancel the operation ?";
                frmIoMessage.btnYes.Text = "Skip";
                frmIoMessage.btnNo.Text = "Cancel";
                if (frmIoMessage.ShowDialog() != DialogResult.Yes)
                    throw new IOException(ex.Message + " : " + file, ex);
                args.AllwaysSkipOnErrorFiles = frmIoMessage.chkAllways.Checked;
            }
        }

        private bool CreateDirectory(string destination, IOBackGroundWorkerProgress prg, IOBackGroundWorkerArguments args)
        {
            try
            {
                Delimon.Win32.IO.Directory.CreateDirectory(destination);
                ++prg.FoldersCopied;
                return true;
            }
            catch (Exception ex)
            {
                IOException ioException = new IOException("Destination : " + destination + Environment.NewLine + ex.Message, ex);
                prg.Errors.Add((Exception)ioException);
                ++prg.FoldersError;
                ++prg.FilesSkipped;
                if (!args.AllwaysSkipOnErrorFolders)
                {
                    frmIOMessage frmIoMessage = new frmIOMessage(IOMessageType.Error, IOMessageIcon.File);
                    frmIoMessage.lblMessage.Text = "Create Folder Error : " + Environment.NewLine + "Destination : " + destination + Environment.NewLine + "Error : " + ex.Message + Environment.NewLine + "Do you want Skip this Folder or Cancel the operation ?";
                    frmIoMessage.btnYes.Text = "Skip";
                    frmIoMessage.btnNo.Text = "Cancel";
                    if (frmIoMessage.ShowDialog() != DialogResult.Yes)
                        throw new IOException(ex.Message + " : " + destination, ex);
                    args.AllwaysSkipOnErrorFolders = frmIoMessage.chkAllways.Checked;
                }
                return false;
            }
        }

        private bool DeleteDirectory(string folder, IOBackGroundWorkerProgress prg, IOBackGroundWorkerArguments args, BackgroundWorker worker, DoWorkEventArgs e)
        {
            if (args.Type == IOBackGroundWorkerType.Delete)
            {
                ++prg.FoldersProcessed;
                prg.CurrentFolder = folder;
                foreach (string str in Delimon.Win32.IO.Directory.GetFiles(folder))
                {
                    ++prg.FilesProcessed;
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return false;
                    }
                    else
                    {
                        this.DeleteFile(str, prg, args);
                        prg.CurrentFile = Delimon.Win32.IO.Path.GetFileName(str);
                        this.ReportProgress(worker, prg);
                    }
                }
                if (args.Recursive)
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return false;
                    }
                    else
                    {
                        foreach (string folder1 in Delimon.Win32.IO.Directory.GetDirectories(folder))
                        {
                            this.DeleteDirectory(folder1, prg, args, worker, e);
                            prg.CurrentFolder = folder1;
                            this.ReportProgress(worker, prg);
                        }
                    }
                }
            }
            try
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return false;
                }
                else
                {
                    if (!Delimon.Win32.IO.Directory.Delete(folder))
                        Helpers.GetLastErrorAndThrowIfFailed(folder);
                    ++prg.FoldersDeleted;
                    return true;
                }
            }
            catch (Exception ex)
            {
                IOException ioException = new IOException("Folder : " + folder + Environment.NewLine + ex.Message, ex);
                prg.Errors.Add((Exception)ioException);
                ++prg.FoldersError;
                ++prg.FilesSkipped;
                if (!args.AllwaysSkipOnErrorFolders)
                {
                    frmIOMessage frmIoMessage = new frmIOMessage(IOMessageType.Error, IOMessageIcon.File);
                    frmIoMessage.lblMessage.Text = "Delete Folder Error : " + Environment.NewLine + "Folder : " + folder + Environment.NewLine + "Error : " + ex.Message + Environment.NewLine + "Do you want Skip this Folder or Cancel the operation ?";
                    frmIoMessage.btnYes.Text = "Skip";
                    frmIoMessage.btnNo.Text = "Cancel";
                    if (frmIoMessage.ShowDialog() != DialogResult.Yes)
                        throw new IOException(ex.Message + " : " + folder, ex);
                    args.AllwaysSkipOnErrorFolders = frmIoMessage.chkAllways.Checked;
                }
                return false;
            }
        }

        public delegate void Completed(IOBackGroundWorkerProgress progress, object sender, RunWorkerCompletedEventArgs e);

        public delegate void ProgressChanged(IOBackGroundWorkerProgress progress, object sender);
    }
}
*/