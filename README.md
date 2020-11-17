# Alethic.KeyShift

KeyShift is a distributed key-value store overlaid on a Kademlia DHT.

When accessing a key with KeyShift, the storage is 'shifted' to the accessing client, and it becomes the primary owner. This means values are always migrated to the cluster member that uses them, and are always closest to the logic.

KeyShift is appropriate for geo-distributed situations where the data is generally only used by a single accessor, and that accessor maintains affinity to a particular location for a significant time. Though values can migrate between nodes: this is overhead. Appropriate examples include: web server state, where the client is generally accessing the same web-server for an extended period of time, but COULD roam to another web server in another region. Or shopping carts, where the user is generally using the application from a single region, but COULD move to another region in the case of the primary region going offline.

## Function

KeyShift functions through the usage of a Kademlia DHT. The implementation is the `Alethic.Kademlia` project. The Kademlia DHT is used to publish `Entry` records for each key which contain the list of host URIs at which the values are available. When a node Gets or Sets a value into KeyShift, that key is searched for within the DHT. If an existing Entry is found, the primary nodes for that entry are contact in order to "shift" the value.

This shifting happens in a number of phases.
+ Node A initiates a GET against Node B.
  + Node B returns the existing data.
  + Node B places the key into a freeze state for 5 seconds. Other accessors are blocked.
  + Node B returns a token.
+ Node A takes the data returned by Node B and stores it locally.
+ Node A publishes a new ownership record into the DHT in order to register himself as the primary contact point for that key.
+ Node A issues a DELETE request against Node B, containing the previously issued token.
  + Node B validates the token, and deletes the value locally.
  + Node B configures Node A as the forward URI.
+ Node A can now access the data.

This ensures during the 'shift' no other accessors can access the data on Node B, as they do not have the freeze token. They block. Once Node A completes the shift and registers himself as the owner, he instructs Node B to release the freeze and configure Node A as the forwarder. Existing requests to Node B get forwarded to Node A. New clients finding the key in the DHT find the new Entry record and consult Node A.

## Kademlia

KeyShift is implemented ontop of `Alethic.Kademlia`, a Very Extensible Kademlia implementation in C#. `Alethic.Kademlia` configuration is not yet completely documented. However, it supports pluggable communication and discovery protocols (UDP being the primary, with UDP multicast for discovery). Alethic.Kademlia supports Protobuf, MessagePack and JSON formatted messages. Integration of KeyShift into a solution will require configuration and potentially extension of Alethic.Kademlia.
