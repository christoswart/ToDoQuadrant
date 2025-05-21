using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ToDoList
{
    public partial class MainWindow : Window
    {
        private const string ToDoFilePath = "todos.json";
        private List<ToDoItem> toDoItems = new();

        public MainWindow()
        {
            InitializeComponent();
            LoadToDos();
        }

        private void AddToDoButton_Click(object sender, RoutedEventArgs e)
        {
            string toDoText = ToDoInput.Text;
            if (!string.IsNullOrEmpty(toDoText) && QuadrantSelector.SelectedItem is ComboBoxItem selectedQuadrant)
            {
                var quadrant = selectedQuadrant.Content.ToString();
                var toDo = new ToDoItem { Text = toDoText, Quadrant = quadrant };
                toDoItems.Add(toDo);
                AddToDoToUI(toDo);
                SaveToDos();
                ToDoInput.Clear();
                QuadrantSelector.SelectedIndex = -1;
            }
        }

        private void AddToDoToUI(ToDoItem toDo)
        {
            var item = new ListBoxItem
            {
                Content = toDo.Text,
                Margin = new Thickness(5),
                Foreground = new SolidColorBrush(Colors.DarkBlue)
            };
            item.PreviewMouseLeftButtonDown += ToDoItem_PreviewMouseLeftButtonDown;
            switch (toDo.Quadrant)
            {
                case "Work":
                    WorkList.Items.Add(item);
                    break;
                case "Business":
                    BusinessList.Items.Add(item);
                    break;
                case "Personal":
                    PersonalList.Items.Add(item);
                    break;
                case "Education":
                    EducationList.Items.Add(item);
                    break;
            }
        }

        private void ToDoItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item)
            {
                DragDrop.DoDragDrop(item, item, DragDropEffects.Move);
            }
        }

        private void Quadrant_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ListBoxItem)))
            {
                var item = e.Data.GetData(typeof(ListBoxItem)) as ListBoxItem;
                if (item == null) return;
                var oldParent = ItemsControl.ItemsControlFromItemContainer(item) as ListBox;
                oldParent?.Items.Remove(item);
                if (sender is ListBox newParent)
                {
                    newParent.Items.Add(item);
                    // Update quadrant in model
                    string newQuadrant = null;
                    if (newParent == WorkList) newQuadrant = "Work";
                    else if (newParent == BusinessList) newQuadrant = "Business";
                    else if (newParent == PersonalList) newQuadrant = "Personal";
                    else if (newParent == EducationList) newQuadrant = "Education";
                    if (newQuadrant != null)
                    {
                        var toDo = toDoItems.FirstOrDefault(t => t.Text == item.Content.ToString() && t.Quadrant != newQuadrant);
                        if (toDo != null)
                        {
                            toDo.Quadrant = newQuadrant;
                            SaveToDos();
                        }
                    }
                }
            }
        }

        private void SaveToDos()
        {
            try
            {
                var json = JsonSerializer.Serialize(toDoItems);
                File.WriteAllText(ToDoFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving ToDos: {ex.Message}");
            }
        }

        private void LoadToDos()
        {
            if (!File.Exists(ToDoFilePath)) return;
            try
            {
                var json = File.ReadAllText(ToDoFilePath);
                var loaded = JsonSerializer.Deserialize<List<ToDoItem>>(json);
                if (loaded != null)
                {
                    toDoItems = loaded;
                    foreach (var toDo in toDoItems)
                        AddToDoToUI(toDo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading ToDos: {ex.Message}");
            }
        }
    }
}