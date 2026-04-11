using System;
using Stateless;

namespace BugWorkflow
{
    public sealed class Bug
    {
        public enum BugState
        {
            New,            // Новый дефект
            Triage,         // Разбор дефектов
            WaitingInfo,    // Нужно больше информации
            Fixing,         // Исправление
            WaitFixApprove, // Ожидание проверки исправления
            Returned,       // Возврат
            Reopened,       // Переоткрытие
            Closed          // Закрытие
        }

        public enum BugTrigger
        {
            StartTriage,
            RequestMoreInfo,
            BackToTriage,
            StartFix,
            FinisFix,
            MarkNotABug,
            MarkWontFix,
            MarkDuplicate,
            MarkCannotReproduce,
            ResolveFix,
            RejectFix,
            Reopen
        }

        private readonly StateMachine<BugState, BugTrigger> _machine;

        public BugState State => _machine.State;

        public Bug()
        {
            _machine = new StateMachine<BugState, BugTrigger>(BugState.New);
            ConfigureStateMachine();
        }

        private void ConfigureStateMachine()
        {
            _machine.OnUnhandledTrigger((state, trigger) =>
            {
                throw new InvalidOperationException(
                    $"Триггер '{trigger}' недопустим в состоянии '{state}'.");
            });

            _machine.Configure(BugState.New)
                .Permit(BugTrigger.StartTriage, BugState.Triage);

            _machine.Configure(BugState.Triage)
                .Permit(BugTrigger.RequestMoreInfo, BugState.WaitingInfo)
                .Permit(BugTrigger.StartFix, BugState.Fixing)
                .Permit(BugTrigger.MarkNotABug, BugState.Closed)
                .Permit(BugTrigger.MarkWontFix, BugState.Closed)
                .Permit(BugTrigger.MarkDuplicate, BugState.Closed);

            _machine.Configure(BugState.WaitingInfo)
                .Permit(BugTrigger.BackToTriage, BugState.Triage)
                .Permit(BugTrigger.StartFix, BugState.Fixing);

            _machine.Configure(BugState.Fixing)
                .Permit(BugTrigger.FinisFix, BugState.WaitFixApprove)
                .Permit(BugTrigger.RequestMoreInfo, BugState.WaitingInfo)
                .Permit(BugTrigger.MarkCannotReproduce, BugState.Returned);

            _machine.Configure(BugState.WaitFixApprove)
                .Permit(BugTrigger.ResolveFix, BugState.Closed)
                .Permit(BugTrigger.RejectFix, BugState.Returned);

            _machine.Configure(BugState.Reopened)
                .Permit(BugTrigger.BackToTriage, BugState.Triage);

            _machine.Configure(BugState.Closed)
                .Permit(BugTrigger.Reopen, BugState.Reopened);
        }

        public void StartTriage()
        {
            _machine.Fire(BugTrigger.StartTriage);
        }

        public void RequestMoreInfo()
        {
            _machine.Fire(BugTrigger.RequestMoreInfo);
        }

        public void BackToTriage()
        {
            _machine.Fire(BugTrigger.BackToTriage);
        }

        public void StartFix()
        {
            _machine.Fire(BugTrigger.StartFix);
        }

        public void FinishFix()
        {
            _machine.Fire(BugTrigger.FinisFix);
        }

        public void MarkNotABug()
        {
            _machine.Fire(BugTrigger.MarkNotABug);
        }

        public void MarkWontFix()
        {
            _machine.Fire(BugTrigger.MarkWontFix);
        }

        public void MarkDuplicate()
        {
            _machine.Fire(BugTrigger.MarkDuplicate);
        }

        public void MarkCannotReproduce()
        {
            _machine.Fire(BugTrigger.MarkCannotReproduce);
        }

        public void ResolveFix()
        {
            _machine.Fire(BugTrigger.ResolveFix);
        }

        public void RejectFix()
        {
            _machine.Fire(BugTrigger.RejectFix);
        }

        public void Reopen()
        {
            _machine.Fire(BugTrigger.Reopen);
        }

        public void CheckFixResult(bool solved)
        {
            if (solved)
                ResolveFix();
            else
                RejectFix();
        }

        public override string ToString()
        {
            return $"State = {State}";
        }
    }

    public static class Program
    {
        public static void Main()
        {
            var bug = new Bug();
            Console.WriteLine("Начальное состояние: " + bug);

            bug.StartTriage();
            Console.WriteLine("После StartTriage: " + bug);

            bug.StartFix();
            Console.WriteLine("После StartFix: " + bug);

            bug.FinishFix();
            Console.WriteLine("После FinishFix: " + bug);

            bug.CheckFixResult(solved: true);
            Console.WriteLine("После CheckFixResult(true): " + bug);

            bug.Reopen();
            Console.WriteLine("После Reopen: " + bug);

            bug.BackToTriage();
            Console.WriteLine("После BackToTriage: " + bug);

            try
            {
                bug.ResolveFix();
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("Ожидаемая ошибка: " + ex.Message);
            }
        }
    }
}
