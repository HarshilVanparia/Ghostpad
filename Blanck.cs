using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Notepad
{
    public partial class Blanck : Form
    {
        private Stack<string> wordStack;
        private bool isUndoing;
        private string currentFileName;
        private bool isSaved;

        private Stack<string> undoStack;
        private Stack<string> redoStack;
        private bool isRedoing;
        private string currentFilePath = string.Empty; // Stores the path of the current file

        public Blanck()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.Text = $"{Path.GetFileNameWithoutExtension("Untitled")} - Ghostpad";
            wordStack = new Stack<string>();
            isUndoing = false;
            currentFileName = null;
            isSaved = true;
            WindowState = FormWindowState.Maximized;

            undoStack = new Stack<string>();
            redoStack = new Stack<string>();
            isRedoing = false;
        }
        public static Blanck b = new Blanck();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        OpenFileDialog openFileDialog = new OpenFileDialog();



        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {

            SaveFile();

        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            words.Clear();

            // Reset the current file path
            currentFileName = null;

            // Mark as saved
            isSaved = true;

            // Update the form title to "Untitled - Ghostpad"
            this.Text = "Untitled - Ghostpad";
        }

        private void newWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            b.Show();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog.FileName = openFileDialog1.FileName;
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            //saveFileDialog.Filter = openFileDialog1.Filter;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllText(saveFileDialog.FileName, words.Text);
                this.Text = $"{Path.GetFileName(saveFileDialog.FileName)} - Ghostpad";

            }
        }
        private void words_change(object sender, EventArgs e)
        {
            if (!isRedoing)
            {
                // Push the current content onto the undo stack when text is changed
                undoStack.Push(words.Text);
                redoStack.Clear(); // Clear the redo stack since we have new changes
            }
            words.Font = new Font(words.Font.FontFamily,22);
        }
        private void appclose(object sender, KeyEventArgs e)
        {
           
            if (e.Control && e.KeyCode == Keys.S)
            {
                if (e.Shift)
                {
                    SaveAsFile();
                }
                else
                {
                    SaveFile();
                }
                e.SuppressKeyPress = true; // Prevent default behavior
            }
            else if (e.Control && e.KeyCode == Keys.O)
            {
                OpenFile();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.Shift && e.KeyCode == Keys.S)
            {
                saveFileDialog.FileName = openFileDialog1.FileName;
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    System.IO.File.WriteAllText(saveFileDialog.FileName, words.Text);
                    this.Text = $"{Path.GetFileName(saveFileDialog.FileName)} - Ghostpad";

                }
            }

            else if (e.Control && e.Shift && e.KeyCode == Keys.N)
            {
                b.Show();

            }

            else if (e.Control && e.KeyCode == Keys.B)
            {
                FontStyle newfontStyle;
                if (words.SelectionFont.Bold)
                {
                    newfontStyle = words.SelectionFont.Style & ~FontStyle.Bold;
                }
                else
                {
                    newfontStyle = words.SelectionFont.Style | FontStyle.Bold;
                }
                words.SelectionFont = new Font(words.SelectionFont, newfontStyle);
            }

            else if (e.KeyCode == Keys.Tab)
            {
                int tabSize = 4;
                string spaces = new string(' ', tabSize);

                // Insert spaces at the current cursor position
                words.SelectedText = spaces;

                // Prevent the default tab behavior
                e.SuppressKeyPress = true;
            }


            else if (e.Control && e.KeyCode == Keys.Add)
            {
                ChangeFontSize(2);
            }
            else if (e.Control && e.KeyCode == Keys.Subtract)
            {
                ChangeFontSize(-2);
            }

            else if (e.Control && e.KeyCode == Keys.Z)
            {
                UndoAction();
                e.SuppressKeyPress = true; 
            }
            else if (e.Control && e.KeyCode == Keys.Y)
            {
                RedoAction();
                e.SuppressKeyPress = true; 
            }

            else if (e.Control && e.KeyCode == Keys.Back)
            {
                RemoveLastWord();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.I)
            {
                InsertImage();
                e.SuppressKeyPress = true; // Prevent default behavior
            }


            else if (e.Control && e.KeyCode == Keys.N)
            {
                words.Clear();

                // Reset the current file path
                currentFileName = null;

                // Mark as saved
                isSaved = true;

                // Update the form title to "Untitled - Ghostpad"
                this.Text = "Untitled - Ghostpad";
            }
        }

        private void SaveFile()
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                SaveAsFile();
            }
            else
            {
                words.SaveFile(currentFilePath, RichTextBoxStreamType.RichText);
                this.Text = Path.GetFileName(currentFilePath) + " - Ghostpad";
            }
        }

        private void SaveAsFile()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Rich Text Format|*.rtf";
                sfd.Title = "Save As";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    currentFilePath = sfd.FileName;
                    words.SaveFile(currentFilePath, RichTextBoxStreamType.RichText);
                    this.Text = Path.GetFileName(currentFilePath) + " - Ghostpad";
                }
            }
        }

        private void OpenFile()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Rich Text Format|*.rtf";
                ofd.Title = "Open File";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    currentFilePath = ofd.FileName;
                    words.LoadFile(currentFilePath, RichTextBoxStreamType.RichText);
                    this.Text = Path.GetFileName(currentFilePath) + " - Ghostpad";
                }
            }
        }

        private void InsertImage()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.gif;*.tiff;*.ico";
                ofd.Title = "Select an Image";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Image img = Image.FromFile(ofd.FileName);

                        Clipboard.SetImage(img);  // Copy the image to the clipboard
                        words.Paste();        // Paste the image from the clipboard into the RichTextBox

                        Clipboard.Clear();        // Clear the clipboard
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred while inserting the image: " + ex.Message);
                    }
                }
            }
        }

        private void ChangeFontSize(float changeAmount)
        {
            // Save the current selection to restore it later
            int selectionStart = words.SelectionStart;
            int selectionLength = words.SelectionLength;

            // Select all text
            words.SelectAll();

            // Apply the font size change to all text
            if (words.SelectionFont != null)
            {
                float currentSize = words.SelectionFont.Size;
                float newSize = currentSize + changeAmount;

                // Set a minimum and maximum font size limit
                if (newSize < 2) newSize = 2; // Minimum font size
                if (newSize > 72) newSize = 72; // Maximum font size

                // Apply the new font size to all text
                words.SelectionFont = new Font(words.SelectionFont.FontFamily, newSize, words.SelectionFont.Style);
            }

            // Restore the original selection
            words.Select(selectionStart, selectionLength);
        }

        private void richbox(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
            {
                int tabSize = 4;
                string spaces = new string(' ', tabSize);
                // Insert spaces at the current cursor position
                words.SelectedText = spaces;

                // Prevent the default tab behavior
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.A)
            {
                words.SelectAll();
            }
            else if (e.Control && e.KeyCode == Keys.C)
            {
                if (words.SelectionLength > 0)
                {
                    words.Copy();
                }
                e.SuppressKeyPress = true;
            }
            
            else if (e.Control && e.KeyCode == Keys.V)
            {
                words.Paste();
                e.SuppressKeyPress = true;
            }
           
            else if (e.Alt && e.KeyCode == Keys.F4)
            {
                this.Close();
            }
        }
        private void UndoAction()
        {
            if (undoStack.Count > 0)
            {
                isRedoing = true;
                // Pop the last state from the undo stack and push it onto the redo stack
                string lastState = undoStack.Pop();
                redoStack.Push(words.Text);
                words.Text = lastState;
                words.SelectionStart = words.Text.Length; // Move cursor to the end
                isRedoing = false;
            }
        }

        private void RedoAction()
        {
            if (redoStack.Count > 0)
            {
                isRedoing = true;
                // Pop the last redo state from the redo stack and apply it
                string lastRedoState = redoStack.Pop();
                undoStack.Push(words.Text);
                words.Text = lastRedoState;
                words.SelectionStart = words.Text.Length; // Move cursor to the end
                isRedoing = false;
            }
        }
        private void RemoveLastWord()
        {
            // Get the current position of the caret
            int cursorPosition = words.SelectionStart;

            if (cursorPosition == 0)
                return; // If the cursor is at the start, there's no word to delete

            // Find the start of the last word by searching backward for the first space or start of the text
            int wordStart = words.Text.LastIndexOf(' ', cursorPosition - 1);
            if (wordStart == -1)
                wordStart = 0; // If no space is found, delete from the start

            // Determine the length of the word to be removed
            int wordLength = cursorPosition - wordStart;

            // Remove the last word
            words.Text = words.Text.Remove(wordStart, wordLength);

            // Set the cursor position to the start of where the word was
            words.SelectionStart = wordStart;
        }



        /*private void SaveFile()
        {
            if (string.IsNullOrEmpty(currentFileName))
            {
                // No file is currently open, show Save dialog
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        currentFileName = saveFileDialog.FileName;
                        File.WriteAllText(currentFileName, words.Text);
                        this.Text = Path.GetFileName(currentFileName) + " - Ghostpad";
                        isSaved = true;
                    }
                }
            }
            else
            {
                // File is already open, save directly
                File.WriteAllText(currentFileName, words.Text);
                this.Text = Path.GetFileName(currentFileName) + " - Ghostpad";
                isSaved = true;
            }
        }*/


       /* private void OpenFile()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    currentFileName = openFileDialog.FileName;
                    words.Text = File.ReadAllText(currentFileName);
                    this.Text = Path.GetFileName(currentFileName) + " - Ghostpad";
                    isSaved = true;
                }
            }
        }*/

        private void closing(object sender, FormClosingEventArgs e)
        {
            if (!isSaved)
            {
                // Prompt the user to save changes
                var result = MessageBox.Show("Do you want to save changes to your text?", "Notepad", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    SaveFile();
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true; // Cancel the closing operation

                }
            }
        }
    }
}
