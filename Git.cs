﻿using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace bot
{
	static class Git
	{
		public static string BaseUrl = "git://github.com/";
		public static string GitRoot = "state.git";

		public static void Init()
		{
			if (!Directory.Exists(GitRoot))
				External.Run("git", "init --bare {0}".F(GitRoot));
		}

		public static string[] GetRemotes()
		{
			return External.Run("git", "--git-dir={0} remote".F(GitRoot))
				.StandardOutput.Lines();
		}

		static Regex refRegex = new Regex("^(?'sha'[0-9a-f]{40}) refs/remotes/(?'alias'[^/]+)/(?'name'.+)$");
		public static Ref[] GetRefs()
		{
			return External.Run("git", "--git-dir={0} show-ref".F(GitRoot)).StandardOutput.Lines()
				.Select(a => refRegex.Match(a))
				.Where(m => m.Success)
				.Select(m => new Ref(m.Groups["alias"].Value, m.Groups["name"].Value, m.Groups["sha"].Value)).ToArray();
		}
		
		public static string[] GetTags()
		{
			return External.Run("git", "--git-dir={0} tag".F(GitRoot)).StandardOutput.Lines();
		}

		public static bool AddRepo(string alias, string githubName)
		{
			return !External.Run("git", "--git-dir={0} remote add {1} {2}{3}".F(
								GitRoot, alias, BaseUrl, githubName)).Failed;
		}

		public static bool RemoveRepo(string alias)
		{
			return !External.Run("git", "--git-dir={0} remote rm {1}".F(
				GitRoot, alias)).Failed;
		}

		public static bool Fetch()
		{
			return !External.Run("git", "--git-dir={0} fetch --all --prune".F(GitRoot)).Failed;
		}

		public static Ref GetMergeBase( Ref a, Ref b )
		{
			var sha = External.Run("git", "--git-dir={0} merge-base {1} {2}".F(GitRoot, a.Sha, b.Sha)).StandardOutput;
			return new Ref("", "(mergebase)",
				sha.Length >= 40 ? sha.Substring(0, 40) : "");
		}

		public static string[] GetCommitsBetween(Ref a, Ref b)
		{
			return External.Run("git", "--git-dir={0} log {1}..{2} --graph --oneline --no-color".F(
				GitRoot, a.Sha, b.Sha)).StandardOutput.Lines();
		}
		
		public static string GetMessage(Ref a)	
		{
			return External.Run("git", "--git-dir={0} log {1} -1 --format=\"%s\"".F(GitRoot, a.Sha)).StandardOutput.Lines().FirstOrDefault();		
		}
		
		public static string GetRemoteUrl(Ref a)
		{
			//TODO: Regex
			return External.Run("git", "--git-dir={0} remote show -n {1}".F(GitRoot, a.Alias)).StandardOutput.Lines()
				.Where( s => s.Contains("Fetch URL: ") ).FirstOrDefault().Replace("Fetch URL: ", "").Trim();
		}
		
		public static string GetRemoteName(Ref a)
		{
			return GetRemoteUrl(a).Replace(BaseUrl, "").Split('/')[0];
		}
		
		public static string GetRemoteRepoName(Ref a)
		{
			return GetRemoteUrl(a).Replace(BaseUrl, "").Split('/')[1];
		}
	}
}
