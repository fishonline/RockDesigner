namespace Rock.Dyn.Msg
{
	public interface TBase
	{
		///
		/// Reads the TObject from the given input protocol.
		///
		void Read(TSerializer tSerializer);

		///
		/// Writes the objects out to the protocol
		///
		void Write(TSerializer tSerializer);
	}
}
