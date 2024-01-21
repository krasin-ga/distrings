namespace Distrings;

public class HashRingBuilder<TNode>
    where TNode : IComparable<TNode>, INode
{
    private readonly IRingConfiguration _ringConfiguration;

    public HashRingBuilder(IRingConfiguration ringConfiguration)
    {
        _ringConfiguration = ringConfiguration;
    }

    public LookupBuilder PartitionWith(Func<PartitioningBuilder, LookupBuilder> func)
    {
        return func(new PartitioningBuilder(_ringConfiguration));
    }

    public class PartitioningBuilder
    {
        private readonly IRingConfiguration _ringConfiguration;

        public PartitioningBuilder(
            IRingConfiguration ringConfiguration)
        {
            _ringConfiguration = ringConfiguration;
        }

        public LookupBuilder ConsistentHashing(IHashAlgorithm hashAlgorithm)
        {
            return new LookupBuilder(
                _ringConfiguration,
                new ConsistentHashing<TNode>(_ringConfiguration, hashAlgorithm));
        }

        public LookupBuilder PingPong()
        {
            return new LookupBuilder(
                _ringConfiguration,
                new PingPong<TNode>(_ringConfiguration));
        }

        public LookupBuilder Custom(IPartitioningStrategy<TNode> partitioningStrategy)
        {
            return new LookupBuilder(
                _ringConfiguration,
                partitioningStrategy);
        }
    }

    public class LookupBuilder
    {
        private readonly IPartitioningStrategy<TNode> _partitioningStrategy;
        private readonly IRingConfiguration _ringConfiguration;

        public LookupBuilder(
            IRingConfiguration ringConfiguration,
            IPartitioningStrategy<TNode> partitioningStrategy)
        {
            _ringConfiguration = ringConfiguration;
            _partitioningStrategy = partitioningStrategy;
        }

        public FinalBuilder LookupWith(Func<LookupVariantsBuilder, FinalBuilder> func)
        {
            return func(new LookupVariantsBuilder(_ringConfiguration, _partitioningStrategy));
        }
    }

    public class LookupVariantsBuilder
    {
        private readonly IPartitioningStrategy<TNode> _partitioningStrategy;
        private readonly IRingConfiguration _ringConfiguration;

        public LookupVariantsBuilder(
            IRingConfiguration ringConfiguration,
            IPartitioningStrategy<TNode> partitioningStrategy)
        {
            _ringConfiguration = ringConfiguration;
            _partitioningStrategy = partitioningStrategy;
        }

        public FinalBuilder BinarySearch()
        {
            return new FinalBuilder(
                _ringConfiguration,
                _partitioningStrategy,
                new BinarySearch<TNode>.Factory());
        }

        public FinalBuilder MemoryLookup()
        {
            return new FinalBuilder(
                _ringConfiguration,
                _partitioningStrategy,
                new MemoryLookup<TNode>.Factory());
        }

        public FinalBuilder Custom(ILookUpStrategyFactory<TNode> lookUpStrategyFactory)
        {
            return new FinalBuilder(
                _ringConfiguration,
                _partitioningStrategy,
                lookUpStrategyFactory);
        }
    }

    public class FinalBuilder
    {
        private readonly ILookUpStrategyFactory<TNode> _lookUpStrategyFactory;
        private readonly IPartitioningStrategy<TNode> _partitioningStrategy;
        private readonly IRingConfiguration _ringConfiguration;

        public FinalBuilder(
            IRingConfiguration ringConfiguration,
            IPartitioningStrategy<TNode> partitioningStrategy,
            ILookUpStrategyFactory<TNode> lookUpStrategyFactory)
        {
            _ringConfiguration = ringConfiguration;
            _partitioningStrategy = partitioningStrategy;
            _lookUpStrategyFactory = lookUpStrategyFactory;
        }

        public HashRing<TNode> CreateRing(IReadOnlyList<TNode> nodes)
        {
            return new HashRing<TNode>(
                _ringConfiguration,
                _partitioningStrategy,
                _lookUpStrategyFactory,
                nodes);
        }
    }
}