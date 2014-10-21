using System;
using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace Fabmunc
{
    public class Simulator : IDisposable
    {
        private readonly string _url;
        private readonly string _login;
        private readonly string _password;
        private readonly string _botName;
        private readonly string _botEmail;
        private readonly Tracer _tracer;
        private IRepository _repository;
        private readonly Random _random = new Random(17); // Why 17? Why not!

        private string _tempPath;

        public Simulator(string url, string login, string password, string botName, string botEmail, Tracer tracer)
        {
            if (botName == null)
            {
                throw new ArgumentNullException("botName");
            }

            _url = url;
            _login = login;
            _password = password;
            _botName = botName;
            _botEmail = botEmail;
            _tracer = tracer;

            _repository = InitializeRepository();

            SetUpRemote();
            FetchFromRepository();
            CreateLocalBranchesFromRemoteTrackingOnes();
        }

        private void CreateLocalBranchesFromRemoteTrackingOnes()
        {
            foreach (var branch in _repository.Branches.Where(b => b.IsRemote))
            {
                var local = _repository.CreateBranch(branch.Name.Substring("origin/".Length), branch.Tip, Sign());

                _tracer.Write("Created local branch '{0}'", local.Name);
            }
        }

        private Signature Sign()
        {
            return new Signature(_botName, _botEmail, DateTimeOffset.Now);
        }

        private void FetchFromRepository()
        {
            _repository.Fetch("origin", new FetchOptions { CredentialsProvider = CredentialsProvider });

            _tracer.Write("Fetched from 'origin'", _url);
        }

        private Credentials CredentialsProvider(string url, string fromUrl, SupportedCredentialTypes types)
        {
            return new UsernamePasswordCredentials { Username = _login, Password = _password };
        }

        private void SetUpRemote()
        {
            _repository.Network.Remotes.Add("origin", _url);

            _tracer.Write("Remote 'origin' configured with '{0}'", _url);
        }

        private IRepository InitializeRepository()
        {
            _tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            var gitDirPath = Repository.Init(_tempPath);
            var repository = new Repository(gitDirPath);

            _tracer.Write("Repository initialized at '{0}'", repository.Info.WorkingDirectory);

            return repository;
        }

        public void SimulateActivity()
        {
            RunRandomAction(_random);
        }

        private void RunRandomAction(Random rand)
        {
            var next = rand.Next(0, 3);

            Branch branch;

            if (next == 0)
            {
                branch = PickRandomExistingBranch(rand);
            }
            else if (next == 1)
            {
                branch = CreateNewBranch(rand);
            }
            else
            {
                _tracer.Write("Watching YouTube videos");
                return;
            }

            _repository.Checkout(branch, new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });

            _tracer.Write("Checked branch '{0}' out", branch.Name);

            next = rand.Next(0, 3);

            if (next == 0)
            {
                CreateRandomCommit();
            }
            else if (next == 1)
            {
                CreateRandomCommit();
                CreateRandomCommit();
            }
            else
            {
                _tracer.Write("Coffee time.");
                return;
            }

            _repository.Network.Push(branch, new PushOptions { CredentialsProvider = CredentialsProvider });
            _tracer.Write("Pushed branch '{0}'", branch.Name);
        }

        private void CreateRandomCommit()
        {
            //TODO: To be randomized
            const string commitMessage = "Another one hits the dust!";

            //TODO: Content of the commit to be randomized
            // - Drop files
            // - Create files (and subfolders)
            // - Alter existing files

            _repository.Commit(commitMessage, Sign(), Sign(), new CommitOptions { AllowEmptyCommit = true });

            _tracer.Write("Added one commit");
        }

        private Branch CreateNewBranch(Random rand)
        {
            var branchName = string.Format("{0}/branch-{1}", _botName, Guid.NewGuid().ToString().Substring(0, 7));

            var commitIds = _repository
                .Commits.QueryBy(new CommitFilter { Since = _repository.Refs.FromGlob("refs/heads/*") })
                .Select(c => c.Id)
                .ToList();

            int whichOne = rand.Next(0, commitIds.Count);

            Branch branch = _repository.CreateBranch(branchName, commitIds[whichOne].Sha);
            var configuredBranch = _repository.Branches.Update(branch, bu => bu.TrackedBranch = "refs/remotes/origin/" + branchName);

            _tracer.Write("Created branch '{0}'", configuredBranch.Name);

            return configuredBranch;
        }

        private Branch PickRandomExistingBranch(Random rand)
        {
            var branches = _repository.Branches.ToList();
            int whichOne = rand.Next(0, branches.Count);

            return branches[whichOne];
        }

        public void Dispose()
        {
            if (_repository == null)
            {
                return;
            }

            _repository.Dispose();
            _repository = null;

            DirectoryHelper.DeleteDirectory(_tempPath);
            _tempPath = null;
        }
    }
}
