#include "EGT.h"

namespace GoldCPP
{

  static size_t FindStringLengthU16(const uint8_t* buff, size_t buffLen, size_t pos)
  {
    const uint16_t *buffEnd = (const uint16_t *)(buff + buffLen);
    const uint16_t *strBuff = (const uint16_t *)(buff + pos);

    for (size_t strLen = 0; strBuff < buffEnd; ++strLen)
    {
      if (*strBuff++ == 0)
        return strLen;
    }

    assert (false);
    return (uint16_t)-1;
  }

  GPSTR_T EgtReader::RawReadCString()
  {
    // First, find out the length of the string
    size_t len = FindStringLengthU16(Input_, InputLen_, InputPos_);

    // Reserve buffer
    uint16_t *u16text = new uint16_t[len];

    // Read string
    for (size_t i = 0; i < len; ++i)
      u16text[i] = RawReadUInt16();
    RawReadUInt16();    // skip terminating null in input stream

    // Convert to target encoding
    GPSTR_T text(len, ' ');
    for (size_t i = 0; i < len; ++i)
    {
      // Replace this check with proper Unicode conversion.
      // Until that is done, we only support the BMP.
      assert((u16text[i] < 0xD800) || (u16text[i] > 0xDBFF));

      text[i] = u16text[i];
    }

    // Clean up
    delete [] u16text;

    // Finished
    return text;
  }

  uint8_t EgtReader::ReadByte()
  {
    return Input_[InputPos_++];
  }

  uint16_t EgtReader::RawReadUInt16()
  {
    // Read a uint16 in little endian.
    // Byte order can change depending on platform.

    // Least significant byte first
    uint16_t b0 = ReadByte(); assert(!EofReached());
    uint16_t b1 = ReadByte();
    return (uint16_t)((b1 << 8) + b0);
  }

  EgtReader::EgtReader(const uint8_t* input, size_t inputLen) :
      InputPos_(0),
      InputLen_(inputLen),
      Input_(input),
      EntryCount_(0),
      EntriesRead_(0)
  {
    FileHeader_ = RawReadCString();
  }

  Entry EgtReader::RetrieveEntry()
  {
    if (RecordComplete())
    {
      return Entry();
    }
    else
    {
      ++EntriesRead_;
      uint8_t type = ReadByte();

      switch (type)
      {
      case 'E':
        return Entry();
      case 'I':
        return Entry(RawReadUInt16());
      case 'S':
        return Entry(RawReadCString());
      case 'B':
        return Entry(ReadByte() != 0);
      case 'b':
        return Entry(ReadByte());
      default:
        return Entry(EntryType::Error);
      }
    }
  }

  bool EgtReader::GetNextRecord()
  {
      // Finish current record
      while (EntriesRead_ < EntryCount_)
        RetrieveEntry();

      // Start next record
      if (ReadByte() == kRecordContentMulti_)
      {
        EntryCount_ = RawReadUInt16();
        EntriesRead_ = 0;
        return true;
      }
      else
      {
        return false;
      }
  }

  GPSTR_T EgtReader::RetrieveString(bool *success)
  {
    Entry e = RetrieveEntry();
    if (e.Type == EntryType::String)
    {
      if (success) *success = true;
      return GPSTR_T(e.val_string);
    }
    else if (success)
      *success = false;

    return GPSTR_T();
  }


  uint16_t EgtReader::RetrieveInt16(bool *success)
  {
    Entry e = RetrieveEntry();
    if (e.Type == EntryType::UInt16)
    {
      if (success) *success = true;
      return e.val_uint16;
    }
    else if (success)
      *success = false;

    return 0;
  }

  bool EgtReader::RetrieveBoolean(bool *success)
  {
    Entry e = RetrieveEntry();
    if (e.Type == EntryType::Boolean)
    {
      if (success) *success = true;
      return e.val_bool;
    }
    else if (success)
      *success = false;

    return false;
  }

  uint8_t EgtReader::RetrieveByte(bool *success)
  {
    Entry e = RetrieveEntry();
    if (e.Type == EntryType::Byte)
    {
      if (success) *success = true;
      return e.val_byte;
    }
    else if (success)
      *success = false;

    return 0;
  }
}
