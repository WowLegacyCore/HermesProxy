using Google.Protobuf;
using Google.Protobuf.Collections;

namespace Framework.Util
{
    public static class ProtobufExtensions
    {
        private static Bgs.Protocol.Variant AddInternalGetRef(this RepeatedField<Bgs.Protocol.Attribute> attributes, string name)
        {
            var attribute = new Bgs.Protocol.Attribute();
            attribute.Name = name;
            attribute.Value = new Bgs.Protocol.Variant();
            attributes.Add(attribute);

            return attribute.Value;
        }

        public static void AddBlob(this RepeatedField<Bgs.Protocol.Attribute> attributes, string name, ByteString blob)
        {
            attributes.AddInternalGetRef(name).BlobValue = blob;
        }

        public static void AddString(this RepeatedField<Bgs.Protocol.Attribute> attributes, string name, string value)
        {
            attributes.AddInternalGetRef(name).StringValue = value;
        }

        public static void AddInt(this RepeatedField<Bgs.Protocol.Attribute> attributes, string name, long value)
        {
            attributes.AddInternalGetRef(name).IntValue = value;
        }
    }
}
