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

        public List<ExerciseResp> All(int pageIndex = 1, int pageSize = 5)
        {
            var exercises = _dbContext.Exercises.Include(item => item.ExerciseLevel)
                    .Include(item => item.ExerciseType).AsQueryable();

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

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName + ".cpp"); // Đường dẫn đến tệp tin cpp
            string compiledFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            try
            {
               var exercise = _dbContext.Exercises.SingleOrDefault(item => item.Id == id);
               File.WriteAllBytes(filePath, exercise.RunFile);

               string[] lines = File.ReadAllLines(filePath);

                if (lines.Length >= 6)
                {
                    lines[5] = cppCode; // Gán nội dung mới vào dòng thứ 10 (chỉ số 9 trong mảng)
                    File.WriteAllLines(filePath, lines); // Ghi lại toàn bộ nội dung file từ mảng dòng đã thay đổi
                }

                string compilerPath = "g++"; // Đường dẫn đến trình biên dịch g++
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

                // Thực thi tệp thực thi và nhận kết quả
                string executionResult = string.Empty;

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
                        // Nếu quá thời gian, throw ra ngoại lệ và xử lý tương ứng
                        throw new TimeoutException("Execution timed out.");
                    }
                }

                return executionResult;

            }
            catch (Exception ex)
            {
                return "False";
            }
            finally
            {
                // Xóa các tệp tin tạm thời
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
    }
}
