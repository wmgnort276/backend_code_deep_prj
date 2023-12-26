using backend.Data;
using backend.Repository;
using backend.RequestModel;
using backend.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
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

        public List<ExerciseResp> All(string userId, int? exerciseLevelId, int? exerciseTypeId, string? keyword, int? pageIndex = 1, int? pageSize = 5)
        {
            var exercises = _dbContext.Exercises.Include(item => item.ExerciseLevel)
                    .Include(item => item.ExerciseType)
                    .Include(item => item.Submissions)
                    .AsQueryable();

            if (exerciseTypeId != null)
            {
                exercises = exercises.Where(item => item.ExerciseTypeId == exerciseTypeId);
            }

            if (exerciseLevelId != null)
            {
                exercises = exercises.Where(item => item.ExerciseLevelId == exerciseLevelId);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                exercises = exercises.Where(item => item.Name.ToLower().Contains(keyword.ToLower()));
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
                IsResolved = item.Submissions.Any(s => s.ExerciseId == item.Id && s.StudentId == userId && s.Status),
            }).ToList();

            //   throw new NotImplementedException();
        }

        public ExerciseResp? GetById(Guid id, string userId)
        {
            var exercise = _dbContext.Exercises.Include(item => item.ExerciseLevel)
                    .Include(item => item.ExerciseType)
                    .SingleOrDefault(item => item.Id == id);


            if (exercise != null)
            {
                var ratings = _dbContext.Rating.Where(rating => rating.ExerciseId == id)
                    .ToList();
                double averageRating = 0;
                int ratingCount = 0;
                if(ratings.Any())
                {
                    averageRating = ratings.Average(item => item.RatingValue);
                    ratingCount = ratings.Count();
                }

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
                    RatingCount = ratingCount,
                    Rating = averageRating,
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
            int DEFAULT_TIME_LIMIT = 10000;
            var cppCode = sourceCode.Code;
            string fileName = Guid.NewGuid().ToString();

            string filePath = Path.Combine(Directory.GetCurrentDirectory() + "\\Sandbox2", fileName + ".cpp");
            string compiledFilePath = Path.Combine(Directory.GetCurrentDirectory() + "\\Sandbox2", fileName);

            string compileResult = string.Empty;
            string executionResult = string.Empty;
            string errorExecutionResult = string.Empty;

            int limitTime = 0;
            int runTime = 0;
            int memory = 0;
            var exercise = _dbContext.Exercises.SingleOrDefault(item => item.Id == id);
            if(exercise != null)
            {
                limitTime = exercise.TimeLimit;
            }
            var isFirstSubmit = CheckUserFirstSubmit(userId, exercise.Id);
            try
            {
               File.WriteAllBytes(filePath, exercise.RunFile);

               string[] lines = File.ReadAllLines(filePath);

                if (lines.Length >= 8)
                {
                    lines[7] = cppCode;
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
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                };

                using (Process compileProcess = new Process())
                {
                    compileProcess.StartInfo = compileProcessStartInfo;
                    compileProcess.Start();
                    compileProcess.WaitForExit();
                    compileResult = compileProcess.StandardError.ReadToEnd();
                    // compile code error
                }
                if(!string.IsNullOrEmpty(compileResult))
                {
                    string cleanedErrorOutput = compileResult.Replace($"{filePath}:", "");
                    throw new Exception(cleanedErrorOutput);
                }


                ProcessStartInfo executionProcessStartInfo = new ProcessStartInfo
                {
                    FileName = compiledFilePath,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                };

                using (Process executionProcess = new Process())
                {
                    // TODO: use Stopwatch to calculate time running instead
                    var startTime = DateTime.Now;
                    Stopwatch stopwatch = new Stopwatch();
                    executionProcess.StartInfo = executionProcessStartInfo;

                    // Start watiching process
                    stopwatch.Start();
                    executionProcess.Start();
                    memory = (int)executionProcess.PrivateMemorySize64;

                    var task = Task.Run(() =>
                    {
                        while (!executionProcess.HasExited)
                        {
                            memory = (int)executionProcess.PrivateMemorySize64;
                            executionProcess.Refresh();
                        }
                    });


                    // TODO: change the time waiting
                    bool isProcessStop = executionProcess.WaitForExit((limitTime > 0) ? limitTime : DEFAULT_TIME_LIMIT);
                    stopwatch.Stop();
                    runTime = (int)stopwatch.ElapsedMilliseconds;

                    if (!isProcessStop)
                    {
                        Console.WriteLine("Not finish yet!");
                        throw new TimeoutException("Execution timed out");
                    }

                    executionResult = executionProcess.StandardOutput.ReadToEnd();
                    errorExecutionResult = executionProcess.StandardError.ReadToEnd();
                    // runTime = (int)(executionProcess.ExitTime - startTime).TotalMilliseconds;
                }

                return executionResult;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return ex.Message;
            }
            finally
            {
                Console.WriteLine("Memory: " + memory);
                // when the answer is right, update data to submission table
                var submission = new SubmissionModel
                {
                    Status = (executionResult == "1"),
                    StudentId = userId,
                    ExerciseId = exercise.Id,
                    Runtime = runTime,
                    Memory = memory,
                };

                AddSubmission(submission);
                
                // Update user score only it is the first time user submit 
                if(executionResult == "1" && isFirstSubmit)
                {
                    AddUserScore(userId, exercise.Score);
                }

                if (System.IO.File.Exists(filePath))
                {
                    // System.IO.File.Delete(filePath);
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
                TimeLimit = exercise.TimeLimit,
            };
        }

        public SubmissionResp AddSubmission(SubmissionModel model)
        {
            var newSubmission = new Submission
            {
                Status = model.Status,
                StudentId = model.StudentId,
                ExerciseId = model.ExerciseId,
                Runtime = model.Runtime,
                Memory = model.Memory,
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

            // To do check if first submission
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
