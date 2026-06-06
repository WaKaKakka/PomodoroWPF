using System;
using System.Collections.ObjectModel;
using System.Linq;
using PomodoroWPF.Infrastructure;
using PomodoroWPF.Models;

namespace PomodoroWPF.Services
{
    public class TaskService : ViewModelBase
    {
        private readonly PersistenceService _persistence;
        private TaskStore _store;

        public ObservableCollection<PomodoroTask> Tasks { get; } = new();

        private string? _currentTaskId;
        public string? CurrentTaskId
        {
            get => _currentTaskId;
            set
            {
                if (SetProperty(ref _currentTaskId, value))
                {
                    RaisePropertyChanged(nameof(CurrentTask));
                    RaisePropertyChanged(nameof(CurrentTaskDisplay));
                    RaisePropertyChanged(nameof(SummaryText));
                    Save();
                }
            }
        }

        public PomodoroTask? CurrentTask =>
            Tasks.FirstOrDefault(t => t.Id == _currentTaskId);

        public string CurrentTaskDisplay
        {
            get
            {
                var task = CurrentTask;
                if (task == null) return "";
                return $"\U0001F4CC {task.Name} ({task.PomodoroDisplay})";
            }
        }

        public int CompletedCount => Tasks.Count(t => t.IsCompleted);
        public int TotalCount => Tasks.Count;
        public string SummaryText => TotalCount > 0
            ? $"\u5df2\u5b8c\u6210 {CompletedCount}/{TotalCount} \u4e2a\u4efb\u52a1"
            : "\u6682\u65e0\u4efb\u52a1";

        public TaskService(PersistenceService persistence)
        {
            _persistence = persistence;
            _store = persistence.LoadTasks();
            _currentTaskId = _store.CurrentTaskId;

            foreach (var task in _store.Tasks)
                Tasks.Add(task);
        }

        public PomodoroTask AddTask(string name, int estimatedPomodoros, Priority priority)
        {
            var task = new PomodoroTask
            {
                Name = name,
                EstimatedPomodoros = Math.Max(1, estimatedPomodoros),
                Priority = priority,
            };
            Tasks.Add(task);
            NotifyCollectionChanged();
            Save();
            return task;
        }

        public void DeleteTask(string id)
        {
            var task = Tasks.FirstOrDefault(t => t.Id == id);
            if (task == null) return;

            Tasks.Remove(task);
            if (_currentTaskId == id)
                CurrentTaskId = null;

            NotifyCollectionChanged();
            Save();
        }

        public void MarkComplete(string id)
        {
            var task = Tasks.FirstOrDefault(t => t.Id == id);
            if (task == null) return;

            task.IsCompleted = true;
            task.PropertyChanged += (s, e) => NotifyCollectionChanged();
            NotifyCollectionChanged();
            Save();
        }

        public void SetCurrentTask(string id)
        {
            CurrentTaskId = id;
        }

        public void IncrementPomodoroOnCurrentTask()
        {
            var task = CurrentTask;
            if (task == null) return;

            task.ActualPomodoros++;
            RaisePropertyChanged(nameof(CurrentTaskDisplay));
            Save();
        }

        private void NotifyCollectionChanged()
        {
            RaisePropertyChanged(nameof(CompletedCount));
            RaisePropertyChanged(nameof(TotalCount));
            RaisePropertyChanged(nameof(SummaryText));
            RaisePropertyChanged(nameof(CurrentTaskDisplay));
        }

        private void Save()
        {
            _store.Tasks = Tasks.ToList();
            _store.CurrentTaskId = _currentTaskId;
            _persistence.SaveTasks(_store);
        }
    }
}
