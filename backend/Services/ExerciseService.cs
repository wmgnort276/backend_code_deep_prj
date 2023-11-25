﻿using backend.Data;
using backend.Repository;
using backend.RequestModel;
using backend.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;

namespace backend.Services
{
    public class ExerciseService : IExerciseRepository
    {
        private readonly MyDbContext _dbContext;

        public ExerciseService(MyDbContext dbContext) {
            _dbContext = dbContext;
        }
        public ExerciseResp Add(ExerciseModel model, byte[] file)
        {

          var exerciseLevels = _dbContext.ExerciseLevels.SingleOrDefault(item => item.Id == model.ExerciseLevelId);

          var newExercise = new Exercise
           {
             Name = model.Name,
             Description = model.Description,
             // CreatedAt = DateTime.Now,
             // ModifiedAt = DateTime.Now,
             ExerciseLevelId = model.ExerciseLevelId,
             ExerciseTypeId = model.ExerciseTypeId,
             RunFile = file,
             HintCode = model.HintCode,
             TimeLimit = model.TimeLimit,
             Score = (exerciseLevels?.Score != null) ? exerciseLevels.Score : 0
          };

            _dbContext.Exercises.Add(newExercise);
            _dbContext.SaveChanges();

            return new ExerciseResp
              {
              Id = newExercise.Id,
              Name = newExercise.Name,
              Description = newExercise.Description,
              CreatedAt = newExercise.CreatedAt,
              HintCode = newExercise.HintCode,
              TimeLimit = newExercise.TimeLimit,
            };
            // throw new NotImplementedException();
        }

        public List<ExerciseResp> All(int? exerciseLevelId, int? exerciseTypeId, string? keyword, int? pageIndex = 1, int? pageSize = 5)
        {
            var exercises = _dbContext.Exercises.Include(item => item.ExerciseLevel)
                    .Include(item => item.ExerciseType).AsQueryable();

            if (exerciseTypeId != null)
            {
                exercises = exercises.Where(item => item.ExerciseTypeId == exerciseTypeId);
            }

            if(!string.IsNullOrEmpty(keyword))
            {
                exercises = exercises.Where(item => item.Description.Contains(keyword));
            }

            //var result = PaginatedList<Exercise>.Create(exercises, pageIndex, pageSize);
            return exercises.Select(item => new ExerciseResp
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                ExerciseLevelId = item.ExerciseLevelId,
                ExerciseTypeId=item.ExerciseTypeId,
                ExerciseLevelName = item.ExerciseLevel.Name,
                ExerciseTypeName = item.ExerciseType.Name,
            }).ToList();

            //   throw new NotImplementedException();
        }

        public ExerciseResp? GetById(Guid id, string userId)
        {
            var exercise = _dbContext.Exercises.Include(item => item.ExerciseLevel)
                    .Include(item => item.ExerciseType).SingleOrDefault(item => item.Id == id);

            if (exercise != null)
            {
                return new ExerciseResp
                {
                    Id = exercise.Id,
                    Name = exercise.Name,
                    Description = exercise.Description,
                    ExerciseLevelId = exercise.ExerciseLevelId,
                    ExerciseTypeId = exercise.ExerciseTypeId,
                    RunFile = exercise.RunFile,
                    ExerciseLevelName = exercise.ExerciseLevel.Name,
                    ExerciseTypeName = exercise.ExerciseType.Name,
                    HintCode = exercise.HintCode,
                    TimeLimit = exercise.TimeLimit,
                };
            }

            return null;
            // throw new NotImplementedException();
        }

        public byte[]? DownLoadrunFille(Guid id)
        {
            var exercise = _dbContext.Exercises.SingleOrDefault(item => item.Id == id);

            if (exercise != null)
            {
                return exercise?.RunFile;
            }

            return null;
        }

        public string Submit(Guid id, string userId, SourceCode sourceCode)
        {
            float DEFAULT_TIME_LIMIT = 5;
            var cppCode = sourceCode.Code;
            string fileName = string.Concat("source", userId);

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName + ".cpp");
            string compiledFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            string executionResult = string.Empty;
            var exercise = _dbContext.Exercises.SingleOrDefault(item => item.Id == id);

            var isFirstSubmit = CheckUserFirstSubmit(userId, exercise.Id);

            try
            {
               File.WriteAllBytes(filePath, exercise.RunFile);

               string[] lines = File.ReadAllLines(filePath);

                if (lines.Length >= 6)
                {
                    lines[5] = cppCode;
                    File.WriteAllLines(filePath, lines);
                }

                string compilerPath = "g++"; 
                string arguments = $"{filePath} -o {compiledFilePath}";

                ProcessStartInfo compileProcessStartInfo = new ProcessStartInfo
                {
                    FileName = compilerPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process compileProcess = new Process())
                {
                    compileProcess.StartInfo = compileProcessStartInfo;
                    compileProcess.Start();
                    compileProcess.WaitForExit();
                }


                ProcessStartInfo executionProcessStartInfo = new ProcessStartInfo
                {
                    FileName = compiledFilePath,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process executionProcess = new Process())
                {
                    executionProcess.StartInfo = executionProcessStartInfo;
                    // Use task to run the execution process
                    var task = Task.Run(() =>
                    {
                        executionProcess.Start();
                        executionResult = executionProcess.StandardOutput.ReadToEnd();                       
                        // executionProcess.WaitForExit(); => no need to do not wait the process
                    });

                    var completed = task.Wait(TimeSpan.FromSeconds((exercise.TimeLimit > 0.0) ? exercise.TimeLimit : DEFAULT_TIME_LIMIT));

                    if (!completed)
                    {
                        throw new TimeoutException("Execution timed out.");
                    }
                }

                return executionResult;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "0";
            }
            finally
            {
                // when the answer is right, update data to submission table
                var submission = new SubmissionModel
                {
                    Status = (executionResult == "1"),
                    StudentId = userId,
                    ExerciseId = exercise.Id,
                };

                AddSubmission(submission);
                
                // Update user score only it is the first time user submit 
                if(executionResult == "1" && isFirstSubmit)
                {
                    AddUserScore(userId, exercise.Score);
                }

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                if (System.IO.File.Exists(compiledFilePath + ".exe"))
                {
                    foreach (var process in Process.GetProcessesByName(fileName))
                    {
                        process.Kill();
                        process.WaitForExit();
                    }

                   System.IO.File.Delete(compiledFilePath + ".exe");

                }
            }

            throw new NotImplementedException();
        }

        public ExerciseResp Edit(ExerciseModel model, byte[]? file)
        {
            // TODO: only level change then we find exercise level
            var exerciseLevels = _dbContext.ExerciseLevels.SingleOrDefault(item => item.Id == model.ExerciseLevelId);
            var exercise = _dbContext.Exercises.SingleOrDefault(item => item.Id == model.Id);

            if(exercise == null)
            {
                throw new Exception("Not found");
            }

            exercise.Name = model.Name;
            exercise.Description = model.Description;
            exercise.ExerciseLevelId = model.ExerciseLevelId;
            exercise.ExerciseTypeId = model.ExerciseTypeId;
            exercise.HintCode = model.HintCode;
            exercise.TimeLimit = model.TimeLimit;
            exercise.Score = (exerciseLevels?.Score != null) ? exerciseLevels.Score : 0;

            if (file != null)
            {
                exercise.RunFile = file;
            }
            _dbContext.SaveChanges();

            return new ExerciseResp
            {
                Name = exercise.Name,
                Description = exercise.Description,
                ExerciseLevelId = exercise.ExerciseLevelId,
                ExerciseTypeId = exercise.ExerciseTypeId,
                HintCode = exercise.HintCode,
                TimeLimit = exercise.TimeLimit
            };
        }

        public SubmissionResp AddSubmission(SubmissionModel model)
        {
            var newSubmission = new Submission
            {
                Status = model.Status,
                StudentId = model.StudentId,
                ExerciseId = model.ExerciseId,
            };

            _dbContext.Add(newSubmission);
            _dbContext.SaveChanges();

            return new SubmissionResp
            {
                Status = newSubmission.Status,
                StudentId = newSubmission.StudentId,
                ExerciseId = newSubmission.ExerciseId
            };
        }

        public void AddUserScore(string userId, int score)
        {

             var currentUser = _dbContext.Users.SingleOrDefault(item => item.Id == userId);
             currentUser.Score += score;
             _dbContext.SaveChanges();
           
        }

        public bool CheckUserFirstSubmit(string userId, Guid exerciseId)
        {
            var submissionFount = _dbContext.Submissions
                .Where(item => item.StudentId == userId && item.ExerciseId == exerciseId && item.Status).ToList();
            if(submissionFount != null && submissionFount.Count() > 0)
            {
                return false;
            }

            return true;
        }
    }
}
