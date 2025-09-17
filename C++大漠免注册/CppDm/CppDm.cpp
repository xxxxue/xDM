#include <iostream>
#include "dm/GetDmObject.h"

int main()
{	
	auto dm = GetDmObject();
	std::cout << "大漠版本:" << dm->Ver();
}


