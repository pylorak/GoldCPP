#include <fstream>
#include <iostream>
#include <string>
#include "../src/SimpleParser.h"
#include "../src/utf8/checked.h"

using namespace GoldCPP;
using namespace std;

void DrawReduction(GPSTR_T &tree, const std::shared_ptr<Reduction> &reduction, int indent)
{
  //This is a simple recursive procedure that draws an ASCII version of the parse
  //tree

  GPSTR_T indentText = GPSTR_C("");

  for (int n = 1; n <= indent; n++)
  {
    indentText += GPSTR_C(" ");
  }

  //=== Display the children of the reduction
  for (size_t n = 0; n < reduction->Branches.Count(); n++)
  {
    switch (reduction->Branches[n]->GetType())
    {
    case Symbol::SymbolType::Nonterminal:
    {
      std::shared_ptr<Reduction> branch = reduction->Branches[n]->ReductionData;

      tree += indentText + GPSTR_C("") + branch->Parent->GetText(false) + GPSTR_C("\n");
      DrawReduction(tree, branch, indent + 1);
      break;
    }
    default:
    {
      GPSTR_T leaf = reduction->Branches[n]->StringData;
      tree += indentText + GPSTR_C("") + leaf + GPSTR_C("\n");
      break;
    }
    } // switch
  }
}

GPSTR_T DrawReductionTree(const std::shared_ptr<Reduction> &Root)
{
  //This procedure starts the recursion that draws the parse tree.
  GPSTR_T tree =Root->Parent->GetText(false);
  tree += GPSTR_C("+-") + Root->Parent->GetText(false) + GPSTR_C("\n");
  DrawReduction(tree, Root, 1);
  return tree;
}

int main(int argc, char* argv[])
{
  // Input files
  // NOTE! Adjust these to existing files or create them first!
  const char *egt_file = "test-grammar.egt";
  const char *script_file = "test-script.txt";

  // Output file
  // This will be overwritten on each run
  const char *output_file = "test-output.txt";

  // Load binary EGT definition
  std::ifstream egtInput(egt_file, std::ios::binary);
  std::vector<char> egtBuffer((std::istreambuf_iterator<char>(egtInput)), (std::istreambuf_iterator<char>()));
  egtInput.close();

  // Load a string to parse
  std::ifstream srcInput(script_file, std::ios::binary);
  GPSTR_T srcStr((std::istreambuf_iterator<char>(srcInput)), (std::istreambuf_iterator<char>()));
  srcInput.close();

  // Output will hold our... well, output.
  GPSTR_T output;
  {
    SimpleParser parser((uint8_t*)egtBuffer.data(), egtBuffer.size());
    if (parser.Parse(srcStr, output))
    {
      output = DrawReductionTree(parser.Root);
    }
  }

  // Convert to UTF-8
  string utf8line;
  utf8::utf16to8(output.begin(), output.end(), back_inserter(utf8line));

  // Write results to a file
  ofstream myfile;
  myfile.open(output_file);
  myfile << utf8line;
  myfile.close();

  // Give at least some visual feedback
  std::cout << "Parse results written to \"test-output.txt\"." << std::endl;

  return 0;
}
