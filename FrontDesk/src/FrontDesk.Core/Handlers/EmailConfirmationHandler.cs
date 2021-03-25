﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using FrontDesk.Core.Aggregates;
using FrontDesk.Core.Events.ApplicationEvents;
using FrontDesk.Core.Interfaces;
using FrontDesk.Core.Specifications;
using MediatR;
using PluralsightDdd.SharedKernel.Interfaces;

namespace FrontDesk.Core.Services
{
  public class EmailConfirmationHandler : INotificationHandler<AppointmentConfirmedEvent>
  {
    private readonly IRepository<Schedule> _scheduleRepository;
    private readonly IReadRepository<Schedule> _scheduleReadRepository;

    private readonly IApplicationSettings _settings;

    public EmailConfirmationHandler(IRepository<Schedule> scheduleRepository, IReadRepository<Schedule> scheduleReadRepository, IApplicationSettings settings)
    {
      _scheduleRepository = scheduleRepository;
      _scheduleReadRepository = scheduleReadRepository;
      _settings = settings;
    }

    public async Task Handle(AppointmentConfirmedEvent appointmentConfirmedEvent, CancellationToken cancellationToken)
    {
      var scheduleSpec = new ScheduleForClinicAndDateWithAppointmentsSpec(_settings.ClinicId, _settings.TestDate);
      // Note: In this demo this only works for appointments scheduled on TestDate
      var schedule = (await _scheduleReadRepository.GetBySpecAsync(scheduleSpec));
      Guard.Against.Null(schedule, nameof(Schedule));

      var appointmentToConfirm = schedule.Appointments.FirstOrDefault(a => a.Id == appointmentConfirmedEvent.AppointmentId);

      appointmentToConfirm.Confirm(appointmentConfirmedEvent.DateTimeEventOccurred);

      await _scheduleRepository.UpdateAsync(schedule);
    }
  }
}
