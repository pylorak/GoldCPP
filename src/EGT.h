#ifndef GOLDCPP_EGT_H
#define GOLDCPP_EGT_H

#include "String.h"
#include <cstdint>
#include <cassert>

namespace GoldCPP
{
  enum class EntryType : char // used to be 'EntryType' class in reference implementation
  {
    Empty = 'E',
    UInt16 = 'I',   // Unsigned, 2 byte
    String = 'S',   // Unicode format, UTF16
    Boolean = 'B',  // 1 Byte, Value is 0 or 1
    Byte = 'b',
    Error = 0
  };

  // Entry is probably worth its own header
  struct Entry   // used to be 'Entry' class in reference implementation
  {
    const EntryType Type;

    GPSTR_T val_string;
    union
    {
      uint16_t val_uint16;
      uint8_t val_byte;
      bool val_bool;
    };

    Entry(EntryType type) :
      Type(type)
    {}

    Entry() :
      Type(EntryType::Empty)
    {}

    Entry(uint16_t val) :
      Type(EntryType::UInt16), val_uint16(val)
    {}

    Entry(const GPSTR_T &str) :
      Type(EntryType::String), val_string(str)
    {}

    Entry(bool val) :
      Type(EntryType::Boolean), val_bool(val)
    {}

    Entry(uint8_t val) :
      Type(EntryType::Byte), val_byte(val)
    {}
  };

  class EgtReader
  {
  private:
    static const char kRecordContentMulti_;

    size_t InputPos_;
    size_t InputLen_;
    const uint8_t* Input_;

    GPSTR_T FileHeader_;
    uint32_t EntryCount_;
    uint32_t EntriesRead_;

    GPSTR_T RawReadCString();
    uint8_t ReadByte();
    uint16_t RawReadUInt16();

  public:

    enum EgtRecord : char
    {
      InitialStates = 'I',
      Symbol = 'S',
      Production = 'R',
      DFAState = 'D',
      LRState = 'L',
      Property = 'p',
      CharRanges = 'c',
      Group = 'g',
      TableCounts = 't'
    };

    bool RecordComplete() const
    {
      return EntriesRead_ >= EntryCount_;
    }

    uint32_t GetEntryCount() const
    {
      return EntryCount_;
    }

    bool EofReached() const
    {
      assert(InputPos_ <= InputLen_);
      return InputPos_ >= InputLen_;
    }

    EgtReader(const uint8_t* input, size_t inputLen);
    Entry RetrieveEntry();
    bool GetNextRecord();
    GPSTR_T RetrieveString(bool *success = NULL);
    uint16_t RetrieveInt16(bool *success = NULL);
    bool RetrieveBoolean(bool *success = NULL);
    uint8_t RetrieveByte(bool *success = NULL);
  };

}

#endif // GOLDCPP_EGT_H

