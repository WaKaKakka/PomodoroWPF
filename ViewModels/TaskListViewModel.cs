using System;
using System.Collections.ObjectModel;
using System.Linq;
using PomodoroWPF.Infrastructure;
using PomodoroWPF.Models;
using PomodoroWPF.Services;

namespace PomodoroWPF.ViewModels
{
    public class TaskListViewModel : ViewModelBase
    {
        private readonly TaskService _taskService;

        public ObservableCollection<PomodoroTask> Tasks => _taskService.Tasks;

        private PomodoroTask? _selectedTask;
        public PomodoroTask? SelectedTask
        {
            get => _selectedTask;
            set => SetProperty(ref _selectedTask, value);
        }

        public string CurrentTaskName => _taskService.CurrentTask?.Name ?? "";
        public string SummaryText => _taskService.SummaryText;

        public RelayCommand AddTaskCommand { get; }
        public RelayCommand DeleteTaskCommand { get; }
        public RelayCommand MarkCompleteCommand { get; }
        public RelayCommand SetAsCurrentCommand { get; }

        public event Action<PomodoroTask>? AddTaskRequested;
        public event Action? TasksChanged;

        public TaskListViewModel(TaskService taskService)
        {
            _taskService = taskService;

            AddTaskCommand = new RelayCommand(() => AddTaskRequested?.Invoke(new PomodoroTask()));
            DeleteTaskCommand = new RelayCommand(DeleteSelected, () => _selectedTask != null);
            MarkCompleteCommand = new RelayCommand(MarkCompleteSelected, () => _selectedTask != null);
            SetAsCurrentCommand = new RelayCommand(SetAsCurrentSelected, () => _selectedTask != null);

            _taskService.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName is nameof(TaskService.SummaryText) or nameof(TaskService.CurrentTaskDisplay))
                {
                    RaisePropertyChanged(nameof(SummaryText));
                    RaisePropertyChanged(nameof(CurrentTaskName));
                    TasksChanged?.Invoke();
                }
            };
        }

        public void AddNewTask(string name, int estimated, Priority priority)
        {
            _taskService.AddTask(name, estimated, priority);
            TasksChanged?.Invoke();
        }

        public void UpdateTask(PomodoroTask task, string name, int estimated, Priority priority)
        {
            task.Name = name;
            task.EstimatedPomodoros = Math.Max(1, estimated);
            task.Priority = priority;
            TasksChanged?.Invoke();
        }

        private void DeleteSelected()
        {
            if (_selectedTask == null) return;
            _taskService.DeleteTask(_selectedTask.Id);
            _selectedTask = null;
            RaisePropertyChanged(nameof(SelectedTask));
            TasksChanged?.Invoke();
        }

        private void MarkCompleteSelected()
        {
            if (_selectedTask == null) return;
            _taskService.MarkComplete(_selectedTask.Id);
            TasksChanged?.Invoke();
        }

        private void SetAsCurrentSelected()
        {
            if (_selectedTask == null) return;
            _taskService.SetCurrentTask(_selectedTask.Id);
            TasksChanged?.Invoke();
        }
    }
}
