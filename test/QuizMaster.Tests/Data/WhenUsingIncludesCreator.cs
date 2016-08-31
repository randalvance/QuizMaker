﻿using Microsoft.EntityFrameworkCore;
using QuizMaker.Data.Core;
using QuizMaster.Data;
using QuizMaster.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace QuizMaster.Tests.Data
{
    public class WhenUsingIncludesCreator : BaseDataTest
    {
        private IncludesCreator<Session> sessionIncludesCreator;

        public WhenUsingIncludesCreator()
        {
            sessionIncludesCreator = new IncludesCreator<Session>();            
        }

        [Fact]
        public void ShouldProperlySplitPropertiesAndReturnTheCorrectCountOfFuncs()
        {
            var resultsWithCollection1 = sessionIncludesCreator.GetFuncsFromExpression(s => s.QuizSessions);
            var resultsWithCollection2 = sessionIncludesCreator.GetFuncsFromExpression(s => s.QuizSessions[0].Quiz);
            var resultsWithCollection3 = sessionIncludesCreator.GetFuncsFromExpression(s => s.SessionQuestions[0].Question.Answers[0].Question);

            var results1 = sessionIncludesCreator.GetFuncsFromExpression(s => s.ApplicationUser);
            var results2 = sessionIncludesCreator.GetFuncsFromExpression(s => s.ApplicationUser.Logins);
            var results3 = sessionIncludesCreator.GetFuncsFromExpression(s => s.ApplicationUser.SecurityStamp.Length);

            Assert.Equal(1, results1.Count());
            Assert.Equal(2, results2.Count());
            Assert.Equal(3, results3.Count());

            Assert.Equal(1, resultsWithCollection1.Count());
            Assert.Equal(2, resultsWithCollection2.Count());
            Assert.Equal(4, resultsWithCollection3.Count());

            var dbContext = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>());

            sessionIncludesCreator.ApplyIncludes(new List<Session>().AsQueryable(), x => x.ApplicationUser);
        }

        [Fact]
        public void ShouldIncludeNavigationProperties()
        {
            var options = CreateNewOptions();

            using (var dbContext = new ApplicationDbContext(options))
            {
                var session = new Session() { SessionStatus = SessionStatus.Done };
                var quiz = new Quiz() { Code = "TEST_QUIZ" };
                var question = new Question { QuestionText = "Test Question" };
                var answer = new Answer() { AnswerText = "Test Answer", Question = question };
                var sessionAnswer = new SessionAnswer() { Answer = answer, IsCorrect = true, AnswerChronology = 0, UserAnswer = "Test Answer" };
                var sessionQuestion = new SessionQuestion { Question = question, DisplayOrder = 1 };
                var quizSession = new QuizSession() { Quiz = quiz };
                var applicationUser = new ApplicationUser { UserName = "TEST_USER" };

                session.QuizSessions.Add(quizSession);
                session.SessionAnswers.Add(sessionAnswer);
                session.SessionQuestions.Add(sessionQuestion);
                session.ApplicationUser = applicationUser;

                dbContext.Sessions.Add(session);
                dbContext.SaveChanges();
            };

            using (var dbContext = new ApplicationDbContext(options))
            {
                var session = sessionIncludesCreator.ApplyIncludes(dbContext.Sessions,
                    s => s.QuizSessions[0].Quiz,
                    s => s.SessionAnswers[0].Answer,
                    s => s.SessionQuestions,
                    s => s.ApplicationUser).FirstOrDefault(s => s.SessionStatus == SessionStatus.Done);

                Assert.NotEmpty(session.QuizSessions);
                Assert.All(session.QuizSessions, s => Assert.NotNull(s.Quiz));

                Assert.NotEmpty(session.SessionAnswers);
                Assert.All(session.SessionAnswers, s => Assert.NotNull(s.Answer));

                Assert.NotEmpty(session.SessionQuestions);
                Assert.NotNull(session.ApplicationUser);
            }
        }
    }
}