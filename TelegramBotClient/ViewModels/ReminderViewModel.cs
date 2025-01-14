﻿using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using TelegramBot;
using TelegramBotClient.Commands;
using TelegramBotClient.Model;
using TelegramBotClient.Properties;

using Duration = Google.Protobuf.WellKnownTypes.Duration;

namespace TelegramBotClient.ViewModels
{
    public sealed class ReminderViewModel : INotifyPropertyChanged
    {
        private readonly long _userId;
        private readonly ObservableCollection<EventReminder> _eventReminders;

        public string Name { get; set; }
        public string Description { get; set; }
        public string Time { get; set; }
        public string RepeatPeriod { get; set; }
        public Command OkCommand { get; private set; }
        public Command CancelCommand { get; private set; }

        public ReminderViewModel(long userId, ObservableCollection<EventReminder> eventReminders)
        {
            _userId = userId;
            _eventReminders = eventReminders;

            OkCommand = new Command(commandParameter =>
            {
                var window = (Window)commandParameter;
                var channel = GrpcChannel.ForAddress(Settings.Default.Address);
                var client = new TelegramEventService.TelegramEventServiceClient(channel);

                if (Time == null || RepeatPeriod == null) return;

                var time = DateTime.Parse(Time);
                var repeatPeriod = TimeSpan.Parse(RepeatPeriod);
                client.AddReminder(new Reminder
                {
                    Id = 0,
                    UserId = _userId,
                    Name = Name,
                    Description = Description,
                    DateTime = Timestamp.FromDateTime(time.ToUniversalTime()),
                    RepeatPeriod = Duration.FromTimeSpan(repeatPeriod)
                });
                _eventReminders.Add(new EventReminder
                {
                    Id = _eventReminders.Max(x => x.Id) + 1,
                    Name = Name,
                    Description = Description,
                    Time = time,
                    RepeatPeriod = repeatPeriod
                });
                window.Close();
            }, _ => true);
            CancelCommand = new Command(commandParameter =>
            {
                var window = (Window)commandParameter;
                window.Close();
            }, _ => true);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
