#ifndef GOLDCPP_VECTOR_H
#define GOLDCPP_VECTOR_H

#include <vector>
#include <cstddef>
#include <cassert>

namespace GoldCPP
{

  const size_t INVALID_IDX = (size_t)-1;

  template <typename T>
  class Vector
  {
  private:
    std::vector<T> vector_;

  public:
    Vector(size_t initSize = 0, const T& val = T()) :
      vector_(initSize, val)
    {}

    virtual ~Vector()
    {}

    void Add(const T &elem)
    {
      vector_.push_back(elem);
    }

    virtual void Clear()
    {
      vector_.clear();
    }

    size_t Count() const
    {
      return vector_.size();
    }

    T& operator[] (size_t index)
    {
      assert(index < vector_.size());
      return vector_[index];
    }

    const T& operator[] (size_t index) const
    {
      assert(index < vector_.size());
      return vector_[index];
    }

    /* Note, unlike STL's at(), this doesn't check if the index is valid. */
    T& GetItemAt (size_t index)
    {
      assert(index < vector_.size());
      return vector_[index];
    }

    /* Note, unlike STL's at(), this doesn't check if the index is valid. */
    const T& GetItemAt (size_t index) const
    {
      assert(index < vector_.size());
      return vector_[index];
    }

    size_t FindIndexOf(const T &val) const
    {
      size_t numItems = Count();
      for (size_t i = 0; i < numItems; ++i)
      {
        if (GetItemAt(i) == val)
          return i;
      }

      return INVALID_IDX;
    }

    bool Contains(const T &val) const
    {
      return (FindIndexOf(val) != INVALID_IDX);
    }

    void Trim()
    {
      vector_.resize(vector_.size());
    }

    void Reserve(size_t nElems)
    {
      vector_.reserve(nElems);
    }

  };

}
#endif // GOLDCPP_VECTOR_H

