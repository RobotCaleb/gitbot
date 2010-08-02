﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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

		public static bool AddRepo(string alias, string githubName)
		{
			return !External.Run("git", "--git-dir={0} remote add {1} {2}{3}".F(
								GitRoot, alias, BaseUrl, githubName)).Failed;
		}

		public static bool Fetch()
		{
			return !External.Run("git", "--git-dir={0} fetch --all").Failed;
		}
	}
}
