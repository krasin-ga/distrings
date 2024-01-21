
[![Distrings Nuget](https://img.shields.io/nuget/v/Distrings?color=1E9400&label=Distrings&style=flat-square)](https://www.nuget.org/packages/Distrings/)

# Distrings 

<img src="https://raw.githubusercontent.com/krasin-ga/distrings/main/assets/distrings.svg" align="right" />
Distrings is a .NET library for constructing a hash ring based on a specified set of nodes.  The primary use of hash ringing includes distributing workload among nodes in a distributed system and implementing algorithms for mapping keys to nodes (e.g., session affinity).

‎ 

‎ 

## Key Features
+ **Flexibility:** All components are customizable, facilitating easy adaptation of the ring mechanism to the architecture and infrastructure of the target project.
+ **Clarity:** Each node is assigned key ranges, enabling the assessment of the resulting distribution against the desired one.
+ **Weighted Nodes:** Nodes can have individual weights, enabling fine-grained control over their impact on share distribution within the hash ring.
+ **Multiple Node Selection:** It is possible to select multiple unique nodes at once (e.g., for replication).

## Core Building Blocks
 `INode`: Node, consisting of an identifier and weight.

 `IRingConfiguration`: Allows adjusting the size of the ring.

 `IHashAlgorithm`: Hashing algorithm.

 `IPartitioningStrategy`: Strategy that divides the given set of nodes into segments and assigns hash ranges to them.

*Built-in Implementations*
- `ConsistentHashing`: A well-known consistent hashing algorithm. Node weight maps to the number of virtual nodes.
- `PingPong`: Ensures even distribution and minimal redistribution but expects nodes to be added in increasing order.

`ILookUpStrategy`: Strategy for locating nodes on the ring.

*Built-in Implementations*

- `BinarySearch`: Uses binary search and allows finding a node in O(log(N)).

- `MemoryLookup`: Requires a constrained ring size and stores all slots in memory, enabling efficient node retrieval in O(1) time.

## Getting Started

### Installation

You can install the library in your project using the following command:


```bash
dotnet add package Distrings
```

### Usage Example
In the example below, a ring of three nodes is created using the consistent hashing algorithm and employs binary search to locate nodes by hash. XXHash is used as the hashing algorithm, which is also used for calculating the hash of string and integer keys and selecting nodes based on this key.


``` C#
using Distrings;
//...

public static class ExampleOfUsage
{
    public static void RunExample()
    {
        var nodes = new[]
        {
            new Node(Identity: "node_01", Weight: 10),
            new Node(Identity: "node_02", Weight: 10),
            new Node(Identity: "node_03", Weight: 30)
        };

        var hashAlgorithm = new XxHashAlgorithm();

        var ring = new HashRingBuilder<Node>(RingConfiguration.Default)
                    .PartitionWith(t => t.ConsistentHashing(hashAlgorithm))
                    .LookupWith(l => l.BinarySearch())
                    .CreateRing(nodes);

        Console.WriteLine(ring);

        var userNode = GetNodeByString(
            stringId: "user@example.com",
            hashAlgorithm,
            ring);

        Console.WriteLine(userNode);

        var idNode = GetNodeByULong(
            id: 10202UL,
            hashAlgorithm,
            ring);

        Console.WriteLine(idNode);
    }

    private static Node GetNodeByString(
        string stringId,
        IHashAlgorithm hashAlgorithm,
        HashRing<Node> ring)
    {
        var maxByteCount = Encoding.UTF8.GetMaxByteCount(stringId.Length);

        const int maxBytesOnStack = 128;
        var userIdBytes = maxByteCount <= maxBytesOnStack
        ? stackalloc byte[maxBytesOnStack]
        : new byte[maxByteCount];

        userIdBytes = userIdBytes[..Encoding.UTF8.GetBytes(stringId, userIdBytes)];
        var userIdHash = hashAlgorithm.CalculateHashCode(userIdBytes);

        return ring.GetNode(hashCode: userIdHash);
    }

    private static Node GetNodeByULong(
        ulong id,
        IHashAlgorithm hashAlgorithm,
        HashRing<Node> ring)
    {
        Span<byte> itemIdBytes = stackalloc byte[8];
        BitConverter.TryWriteBytes(itemIdBytes, id);
        var userIdHash = hashAlgorithm.CalculateHashCode(itemIdBytes);

        return ring.GetNode(hashCode: userIdHash);
    }
}


public class XxHashAlgorithm : IHashAlgorithm
{
    public ulong CalculateHashCode(ReadOnlySpan<byte> bytes)
    {
        Span<byte> destination = stackalloc byte[8];

        //from System.IO.Hashing NugetPackage 
        System.IO.Hashing.XxHash64.Hash(bytes, destination);

        return BitConverter.ToUInt64(destination);
    }
}
```

‎ 

>**_Notes:_**  
 1. In practice, hashing algorithms for nodes and keys can be different
 2. It is not recommended to use plain auto-incrementing integer keys for node selection, as this would result in uneven node selection. Instead, it is suggested to use hashed values for key selection
 3. For consistency, all ring parameters should be the same across all application instances


## License

This project is licensed under the [MIT license](LICENSE).
