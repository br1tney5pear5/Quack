using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
///BOT
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quack.Models.Account;

using Quack.Models;

namespace Quack
{
    public class BotManager : IHostedService {
        class Bot {
            public int ID;
            public int userID;
            public int count;
            public int minWords;
            public int maxWords;
            public float postProbability;

            public string[] words;
            public float[,] markovMatrix;

            public Bot(BotModel model) {
                ID = model.ID;
                userID = model.userID ?? -1;

                minWords = model.minWords;
                maxWords = model.maxWords;
                postProbability = model.postProbability;

                model.seed = Encoding.ASCII.GetString(
                  Encoding.Convert(Encoding.UTF8,
                    Encoding.GetEncoding(
                      Encoding.ASCII.EncodingName,
                        new EncoderReplacementFallback(string.Empty),
                        new DecoderExceptionFallback()
                      ),
                    Encoding.UTF8.GetBytes(model.seed)
                  )
                );
                model.seed = model.seed.Replace(".", " . ");
                model.seed = model.seed.Replace(",", " , ");
                model.seed = model.seed.Replace("!", " ! ");
                model.seed = model.seed.Replace("?", " ! ");
                model.seed = model.seed.Replace("\r", " ");
                model.seed = model.seed.Replace("\n", " ");
                model.seed = model.seed.ToLower();

                var tempseed = model.seed
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                words = tempseed.Distinct().ToArray();
                count = words.Count();
                string[] seed = tempseed.ToArray();
                markovMatrix = new float[count, count];

                for(int i = 1; i < seed.Count(); i++) {
                    int currIndex = Array.IndexOf(words,seed[i  ]);
                    int prevIndex = Array.IndexOf(words,seed[i-1]);
                    markovMatrix[currIndex, prevIndex] += 1.0f;
                }

                // matrix normalizataion
                for(int x = 0; x < count; x++) {
                    float columnCount = 0;
                    for(int y = 0; y < count; y++)
                        columnCount += markovMatrix[x,y];

                    if(columnCount != 0){
                        for(int y = 0; y < count; y++){
                            markovMatrix[x,y] /= columnCount;

                        }
                    }
                }
            }
        }
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BotManager> _logger;
        private Timer _timer;
        private List<Bot> bots;
        private Random random;

        public BotManager(ILogger<BotManager> logger,
                          IServiceScopeFactory scopeFactory)
        {
            random = new Random();
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken) {
            using (var scope = _scopeFactory.CreateScope()) {
                var _context =
                    scope.ServiceProvider.GetRequiredService<QuackDbContext>();
                var _userManager =
                    scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var unassignedBots = _context.Bot.Where(model => model.userID == null);
                foreach(var b in unassignedBots) {
                    string botName = "Bot" + b.ID.ToString() + "_" + random.Next(999999).ToString();
                    string botMail = botName + "@domain.com";
                    var user = new User{
                        UserName = botName,
                        Email = botMail,
                        avatarUrl = "https://i.pravatar.cc/100?img=" + random.Next(1,71).ToString()
                    };
                    var result = await _userManager.CreateAsync(user, "Password123");
                    if(!result.Succeeded) {
                        _logger.LogWarning("Failed to create account for bot " +
                                           b.ID.ToString());
                    }
                    b.userID = Convert.ToInt32(_userManager.Users
                                            .Where(u => u.Email == botMail)
                                            .FirstOrDefault().Id );
                }
                _context.SaveChanges();

                bots = _context.Bot.Select(model => new Bot(model)).ToList();
            }

            _logger.LogInformation("Bot Start");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
            return;
        }
        public void DoWork(object state) {
          _logger.LogInformation("Bot run");

          foreach(Bot bot in bots) {
            float postDet = (float)random.NextDouble();
            if(postDet < bot.postProbability){
              string generated = "";
              int wordsCount = random.Next(bot.minWords, bot.maxWords);

              bool capitalLetter = true;
              bool omitSpace = true;

              int wordIndex = Array.IndexOf(bot.words, ".");
              if(wordIndex < 0)
                  wordIndex = random.Next(bot.count);

              for(int i=0; i < wordsCount; i++) {
                  float r = (float)random.NextDouble();
                  float temp = 0.0f;

                  int resultIndex = random.Next(bot.count);

                  for(int y=0; y < bot.count; y++) {
                      temp += bot.markovMatrix[y, wordIndex];
                      if(temp > r) {
                          var w = bot.words[y];
                          if(capitalLetter) {
                              var wChArr = w.ToCharArray();
                              wChArr[0] = char.ToUpper(wChArr[0]);
                              w = new string(wChArr);
                              capitalLetter = false;
                          }

                          if(w != "." && w != "," && w != "!") {
                              if(omitSpace) {
                                  omitSpace = false;
                              }else{
                                  generated += " ";
                                  generated += w;
                              }
                          } else {
                              if(w == ".") capitalLetter = true;
                              omitSpace = true;
                              generated += w + " ";
                          }

                          resultIndex = y;
                          break;
                      }
                  }
                  wordIndex = resultIndex;
              }

              using (var scope = _scopeFactory.CreateScope()) {
                var _context =
                    scope.ServiceProvider.GetRequiredService<QuackDbContext>();
                if(_context.Post.Count() < 10 || random.NextDouble() > 0.85) {
                  var postContent = new PostContent{text = generated};
                  var post = new Post{
                      content = postContent,
                      authorID = bot.userID,
                      datePublished = DateTime.UtcNow
                  };

                  _context.Post.Add(post);
                  _context.SaveChanges();
                } else {
                  var postID = _context.Post
                      .OrderByDescending(p => p.datePublished)
                      .Skip(random.Next(5))
                      .FirstOrDefault().ID;

                  var comment = new Comment{
                      text = generated,
                      postID = postID,
                      authorID = bot.userID,
                      datePublished = DateTime.UtcNow
                  };

                  _context.Comment.Add(comment);
                  _context.SaveChanges();
                }
              }
            }
          }
        }
        public Task StopAsync(CancellationToken cancellationToken) {
            _logger.LogInformation("Bot Stop");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
        public void Dispose() {
            _timer?.Dispose();
        }
    }
}
