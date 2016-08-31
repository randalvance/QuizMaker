﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using QuizMaster.Common;
using QuizMaster.Data.Constants;
using QuizMaster.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizMaster.Data.Extensions
{
    public static class DataSeeder
    {
        public async static void SeedDataAsync(this IApplicationBuilder app, 
            string initialQuizFolder, 
            ApplicationDbContext appDbContext,
            UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,
            Options.IdentityOptions identityOptions)
        {
            var superAdminUser = await userManager.FindByNameAsync(identityOptions.SuperAdminDefaultUserName);

            if (superAdminUser == null)
            {
                var user = new ApplicationUser()
                {
                    UserName = identityOptions.SuperAdminDefaultUserName,
                    Email = identityOptions.SuperAdminDefaultUserName
                };

                var role = new ApplicationRole()
                {
                    Name = IdentityConstants.SuperAdministratorRoleName
                };

                await userManager.CreateAsync(user, identityOptions.SuperAdminDefaultPassword);
                await roleManager.CreateAsync(role);
                await userManager.AddToRoleAsync(user, role.Name);

                await AddExampleQuestionsAsync(appDbContext, user, initialQuizFolder);
            }

            await AddInitialSettingsAsync(appDbContext);
        }

        private static async Task AddExampleQuestionsAsync(ApplicationDbContext appDbContext, ApplicationUser superAdminUser, string initialQuizFolder)
        {
            List<QuizCategory> quizCategories = new List<QuizCategory>()
            {
                new QuizCategory { Code = "OneOrMany", Name = "One or Many", Description = "" },
                new QuizCategory { Code = "WordTypes", Name = "Word Types", Description = "" },
                new QuizCategory { Code = "SentenceParts", Name = "Sentence Parts", Description = "" }
            };
            List<QuizGroup> quizGroups = new List<QuizGroup>()
            {
                new QuizGroup { Code = "ADVERBS", Name="Adverbs", QuizCategory = quizCategories[1] },
                new QuizGroup { Code = "COMPARATIVE_FORM", Name = "Comparative Forms", QuizCategory = quizCategories[1] },
                new QuizGroup { Code = "SUPERLATIVE_FORM", Name = "Superlative Forms", QuizCategory = quizCategories[1] },
                new QuizGroup { Code = "SUBJECTS", Name = "Subjects", QuizCategory = quizCategories[2] },
                new QuizGroup { Code = "IRREGULAR_NOUNS", Name = "Irregular Nouns", QuizCategory = quizCategories[1] },
                new QuizGroup { Code = "IS_AND_ARE", Name = "Is And Are", QuizCategory = quizCategories[0] },
                new QuizGroup { Code = "SINGULAR_OR_PLURAL", Name = "Singular Or Plural", QuizCategory = quizCategories[0] }
            };
            appDbContext.QuizGroups.AddRange(quizGroups);

            await appDbContext.SaveChangesAsync();

            List<Quiz> initialQuizes = QuestionParser.ParseQuizFiles(initialQuizFolder);
            var quizGroupCache = new Dictionary<string, Guid>();

            foreach(var group in quizGroups)
            {
                quizGroupCache[group.Code] = group.QuizGroupId;
            }

            foreach(var quiz in initialQuizes)
            {
                quiz.QuizGroupId = quizGroupCache[quiz.QuizGroupCode];
            }

            appDbContext.Quizes.AddRange(initialQuizes);
            await appDbContext.SaveChangesAsync();
        }

        private static async Task AddInitialSettingsAsync(ApplicationDbContext appDbContext)
        {
            var initialSettings = appDbContext.ApplicationSettings.ToList();

            if (initialSettings.FirstOrDefault(x => x.Key == "Sessions.RecommendedSessionCountPerDay") == null)
            {
                ApplicationSetting setting = new ApplicationSetting()
                {
                    ApplicationSettingValueType = ApplicationSettingValueType.Int,
                    Key = "Sessions.RecommendedSessionCountPerDay",
                    Name = "Recommend Sessions Per Day",
                    Value = "15"
                };

                appDbContext.ApplicationSettings.Add(setting);
            }

            if (initialSettings.FirstOrDefault(x => x.Key == "Quiz.PassingGrade") == null)
            {
                ApplicationSetting setting = new ApplicationSetting()
                {
                    ApplicationSettingValueType = ApplicationSettingValueType.Double,
                    Key = "Quiz.PassingGrade",
                    Name = "Passing Grade",
                    Value = "70"
                };

                appDbContext.ApplicationSettings.Add(setting);
            }

            await appDbContext.SaveChangesAsync();
        }
    }
}
