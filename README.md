# Alethic.KeyShift

KeyShift is a distributed key-value store based on a Kademlia DHT.

When accessing a key with KeyShift, the storage is 'shifted' to the accessing client, and it becomes the primary owner. This means values are always migrated to the cluster member that uses them, and are always closest to the logic.

KeyShift is appropriate for geo-distributed situations where the data is generally only used by a single accessor, and that accessor maintains affinity to a particular location for a significant time. Though values can migrate between nodes: this is overhead. Appropriate examples include: web server state, where the client is generally accessing the same web-server for an extended period of time, but COULD roam to another web server in another region. Or shopping carts, where the user is generally using the application from a single region, but COULD move to another region in the case of the primary region going offline.