using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BugWorkflow;

namespace BugWorkflow.Tests
{
    [TestClass]
    public class BugTests
    {
        [TestMethod]
        public void NewBug_ShouldHaveInitialStateNew()
        {
            var bug = new Bug();

            Assert.AreEqual(Bug.BugState.New, bug.State);
            Assert.AreEqual(Bug.ReturnReason.None, bug.CurrentReturnReason);
            Assert.AreEqual(Bug.CloseReason.None, bug.CurrentCloseReason);
        }

        [TestMethod]
        public void StartTriage_FromNew_ShouldMoveToTriage()
        {
            var bug = new Bug();

            bug.StartTriage();

            Assert.AreEqual(Bug.BugState.Triage, bug.State);
        }

        [TestMethod]
        public void RequestMoreInfo_FromTriage_ShouldMoveToWaitingInfo()
        {
            var bug = new Bug();
            bug.StartTriage();

            bug.RequestMoreInfo();

            Assert.AreEqual(Bug.BugState.WaitingInfo, bug.State);
        }

        [TestMethod]
        public void BackToTriage_FromWaitingInfo_ShouldMoveToTriage()
        {
            var bug = new Bug();
            bug.StartTriage();
            bug.RequestMoreInfo();

            bug.BackToTriage();

            Assert.AreEqual(Bug.BugState.Triage, bug.State);
        }

        [TestMethod]
        public void StartFix_FromTriage_ShouldMoveToFixing()
        {
            var bug = new Bug();
            bug.StartTriage();

            bug.StartFix();

            Assert.AreEqual(Bug.BugState.Fixing, bug.State);
        }

        [TestMethod]
        public void StartFix_FromWaitingInfo_ShouldMoveToFixing()
        {
            var bug = new Bug();
            bug.StartTriage();
            bug.RequestMoreInfo();

            bug.StartFix();

            Assert.AreEqual(Bug.BugState.Fixing, bug.State);
        }

        [TestMethod]
        public void FinishFix_FromFixing_ShouldMoveToWaitFixApprove()
        {
            var bug = new Bug();
            bug.StartTriage();
            bug.StartFix();

            bug.FinishFix();

            Assert.AreEqual(Bug.BugState.WaitFixApprove, bug.State);
        }

        [TestMethod]
        public void ResolveFix_FromWaitFixApprove_ShouldCloseBugWithFixedReason()
        {
            var bug = new Bug();
            bug.StartTriage();
            bug.StartFix();
            bug.FinishFix();

            bug.ResolveFix();

            Assert.AreEqual(Bug.BugState.Closed, bug.State);
            Assert.AreEqual(Bug.CloseReason.Fixed, bug.CurrentCloseReason);
        }

        [TestMethod]
        public void RejectFix_FromWaitFixApprove_ShouldMoveToReturnedWithFixRejectedReason()
        {
            var bug = new Bug();
            bug.StartTriage();
            bug.StartFix();
            bug.FinishFix();

            bug.RejectFix();

            Assert.AreEqual(Bug.BugState.Returned, bug.State);
            Assert.AreEqual(Bug.ReturnReason.FixRejected, bug.CurrentReturnReason);
        }

        [TestMethod]
        public void CheckFixResult_True_FromWaitFixApprove_ShouldCloseBug()
        {
            var bug = new Bug();
            bug.StartTriage();
            bug.StartFix();
            bug.FinishFix();

            bug.CheckFixResult(true);

            Assert.AreEqual(Bug.BugState.Closed, bug.State);
            Assert.AreEqual(Bug.CloseReason.Fixed, bug.CurrentCloseReason);
        }

        [TestMethod]
        public void CheckFixResult_False_FromWaitFixApprove_ShouldReturnBug()
        {
            var bug = new Bug();
            bug.StartTriage();
            bug.StartFix();
            bug.FinishFix();

            bug.CheckFixResult(false);

            Assert.AreEqual(Bug.BugState.Returned, bug.State);
            Assert.AreEqual(Bug.ReturnReason.FixRejected, bug.CurrentReturnReason);
        }

        [TestMethod]
        public void MarkNotABug_FromTriage_ShouldCloseBugWithNotABugReason()
        {
            var bug = new Bug();
            bug.StartTriage();

            bug.MarkNotABug();

            Assert.AreEqual(Bug.BugState.Closed, bug.State);
            Assert.AreEqual(Bug.ReturnReason.NotABug, bug.CurrentReturnReason);
        }

        [TestMethod]
        public void MarkWontFix_FromTriage_ShouldCloseBugWithWontFixReason()
        {
            var bug = new Bug();
            bug.StartTriage();

            bug.MarkWontFix();

            Assert.AreEqual(Bug.BugState.Closed, bug.State);
            Assert.AreEqual(Bug.ReturnReason.WontFix, bug.CurrentReturnReason);
        }

        [TestMethod]
        public void MarkDuplicate_FromTriage_ShouldCloseBugWithDuplicateReason()
        {
            var bug = new Bug();
            bug.StartTriage();

            bug.MarkDuplicate();

            Assert.AreEqual(Bug.BugState.Closed, bug.State);
            Assert.AreEqual(Bug.ReturnReason.Duplicate, bug.CurrentReturnReason);
        }

        [TestMethod]
        public void MarkCannotReproduce_FromFixing_ShouldMoveToReturned()
        {
            var bug = new Bug();
            bug.StartTriage();
            bug.StartFix();

            bug.MarkCannotReproduce();

            Assert.AreEqual(Bug.BugState.Returned, bug.State);
            Assert.AreEqual(Bug.ReturnReason.CannotReproduce, bug.CurrentReturnReason);
        }

        [TestMethod]
        public void Reopen_FromClosed_ShouldMoveToReopened()
        {
            var bug = new Bug();
            bug.StartTriage();
            bug.MarkNotABug();

            bug.Reopen();

            Assert.AreEqual(Bug.BugState.Reopened, bug.State);
        }

        [TestMethod]
        public void BackToTriage_FromReopened_ShouldMoveToTriage()
        {
            var bug = new Bug();
            bug.StartTriage();
            bug.MarkDuplicate();
            bug.Reopen();

            bug.BackToTriage();

            Assert.AreEqual(Bug.BugState.Triage, bug.State);
        }

        [TestMethod]
        public void EnteringTriage_AfterReopen_ShouldResetReasons()
        {
            var bug = new Bug();
            bug.StartTriage();
            bug.MarkWontFix();
            bug.Reopen();

            bug.BackToTriage();

            Assert.AreEqual(Bug.BugState.Triage, bug.State);
            Assert.AreEqual(Bug.ReturnReason.None, bug.CurrentReturnReason);
            Assert.AreEqual(Bug.CloseReason.None, bug.CurrentCloseReason);
        }

        [TestMethod]
        public void StartFix_FromNew_ShouldThrowInvalidOperationException()
        {
            var bug = new Bug();

            Assert.ThrowsException<InvalidOperationException>(() => bug.StartFix());
        }

        [TestMethod]
        public void ResolveFix_FromFixingWithoutFinishFix_ShouldThrowInvalidOperationException()
        {
            var bug = new Bug();
            bug.StartTriage();
            bug.StartFix();

            Assert.ThrowsException<InvalidOperationException>(() => bug.ResolveFix());
        }

        [TestMethod]
        public void Reopen_FromReturned_ShouldThrowInvalidOperationException()
        {
            var bug = new Bug();
            bug.StartTriage();
            bug.StartFix();
            bug.MarkCannotReproduce();

            Assert.ThrowsException<InvalidOperationException>(() => bug.Reopen());
        }
    }
}
