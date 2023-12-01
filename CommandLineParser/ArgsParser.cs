namespace CommandLineParser {

    public enum TagMatchType {
        Switch,
        Singal,
        Multi,
        ManyMulti
    }

    /// <summary>
    /// 参数解析结果
    /// </summary>
    public class ArgsParseResult(Dictionary<string, bool> switchArguments, Dictionary<string, string> singleArguments, Dictionary<string, string[]> multiArguments, Dictionary<string, string[][]> manyMultiArgument) {
       
        public readonly Dictionary<string, bool> SwitchArguments = switchArguments;
        public readonly Dictionary<string, string> SingleArguments = singleArguments;
        public readonly Dictionary<string, string[]> MultiArguments = multiArguments;
        public readonly Dictionary<string, string[][]> ManyMultiArgument = manyMultiArgument;

        /// <summary>
        /// 从解析结果中获取参数值
        /// </summary>
        /// <param name="tag">参数标记</param>
        /// <returns>当参数成功被解析时，返回参数值；否则返回null</returns>
        public object? TryGet(string tag) {
            if (SwitchArguments.TryGetValue(tag, out var sswitch)) {
                return sswitch;
            }else if (SingleArguments.TryGetValue(tag, out var single)) {
                return single;
            }else if ((MultiArguments.TryGetValue(tag,out var multi))) {
                return multi;
            }else if(ManyMultiArgument.TryGetValue(tag, out var manyMulti)) {
                return manyMulti;
            }
            return null;
        }

        /// <inheritdoc cref="TryGet(string)"/>
        public object? this[string tag] {
            get => TryGet(tag);
        }
    }

    /// <summary>
    /// 参数匹配组
    /// </summary>
    /// <param name="tag">参数标记</param>
    /// <param name="type">匹配类型</param>
    /// <param name="require">是否为必须参数(当匹配类型为Switch时此项无效)</param>
    public class TagMatchGroup(string tag, TagMatchType type, bool require = true) {
        public string Tag = tag;
        public TagMatchType Type = type;
        public bool Require = require;
    }

    /// <summary>
    /// 初始化解析器
    /// </summary>
    /// <param name="matchGroups">匹配组</param>
    public class ArgsParser(params TagMatchGroup[] matchGroups) {

        private readonly List<TagMatchGroup> _matchGroups = [.. matchGroups];
        private readonly List<string> _tags = matchGroups.Select(group => group.Tag).ToList();
        private readonly List<string> _args = [];

        private readonly Dictionary<string, bool> _switchArguments = [];
        private readonly Dictionary<string, string> _singleArguments = [];
        private readonly Dictionary<string, string[]> _multiArguments = [];
        private readonly Dictionary<string, string[][]> _manyMultiArguments = [];

        /// <summary>
        /// 对参数进行解析
        /// </summary>
        /// <param name="args">参数</param>
        /// <returns>解析结果</returns>
        /// <exception cref="ArgumentException">必须参数未被成功解析</exception>
        public ArgsParseResult Parse(string[] args) {
            _args.AddRange(args);
            _matchGroups.ForEach(group => {
                switch (group.Type) {
                    case TagMatchType.Switch:
                        ParseSwitch(group.Tag);
                        break;
                    case TagMatchType.Singal:
                        ParseSingle(group.Tag, group.Require);
                        break;
                    case TagMatchType.Multi:
                        ParseMulti(group.Tag, group.Require);
                        break;
                    case TagMatchType.ManyMulti:
                        ParseManyMulti(group.Tag, group.Require);
                        break;
                }
            });
            return new ArgsParseResult(_switchArguments, _singleArguments, _multiArguments, _manyMultiArguments);
        }

        private void ParseSwitch(string tag) {
            _switchArguments.Add(tag, _args.Contains(tag));
        }

        private void ParseSingle(string tag, bool require) {
            var index = _args.IndexOf(tag);
            if (index == -1) {
                if (require) {
                    throw new ArgumentException($"Missing tag '{tag}'", tag);
                } else {
                    return;
                }
            }
            var single = _args.ElementAtOrDefault(index + 1);
            if (single is null || _tags.Contains(single)) {
                throw new ArgumentException($"Missing argument of '{tag}'", tag);
            }
            _singleArguments.Add(tag, single);
        }

        private int GetOneMulit(string tag, bool require, out List<string>? multi, int startIndex = 0) {
            var index = _args.IndexOf(tag, startIndex);
            if (index == -1) {
                multi = null;
                return -1;
            }

            multi = [];
            for (var next = _args.ElementAtOrDefault(++index);
                next is not null && !_tags.Contains(next);
                next = _args.ElementAtOrDefault(++index)) {
                multi.Add(next);
            }
            return index;
        }

        private void ParseMulti(string tag, bool require) {
            _ = GetOneMulit(tag, require, out List<string>? multi);
            if (multi is null) {
                if (require) {
                    throw new ArgumentException($"Missing tag '{tag}'", tag);
                }
                return;
            }
            _multiArguments.Add(tag, [.. multi]);
        }

        private void ParseManyMulti(string tag, bool require) {
            var multies = new List<string[]>();
            for (int nextIndex = GetOneMulit(tag, require, out List<string>? multi);
                nextIndex != -1;
                nextIndex = GetOneMulit(tag, require, out multi, nextIndex)) {
                multies.Add([.. multi]);
            }
            if (require && multies.Count == 0) {
                throw new ArgumentException($"Missing tag '{tag}'", tag);
            }
            _manyMultiArguments.Add(tag, [.. multies]);
        }

    }
}
