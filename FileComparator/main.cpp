#include <iostream> 
#include <chrono>
#include <cstdio>
#include <algorithm>
#include<fstream>
#include<string>
#include <vector>
#include<stack>
#include <sstream>
#include<cctype>
using namespace std;
using namespace chrono;

int main(int argc, char* argv[])
{
	//使用chrono 高精度计时
	auto start = high_resolution_clock::now();

	//异常信息_传参个数有误输出
	if (argc != 3) {
		cerr << "命令行格式错，请使用格式：\n程序名 文件名1 文件名2\n";
		return 1;
	}

	ifstream txt1, txt2;
	txt1.open(argv[1]);
	txt2.open(argv[2]);

	//异常信息_文件无法打开
	if (!txt1.is_open()) {
		cerr << "Error opening file " << argv[1];
		txt1.clear();
		return 2;
	}
	if (!txt2.is_open()) {
		cerr << "Error opening file " << argv[2];
		txt1.clear();
		txt2.clear();
		return 2;
	}

	stack<char> stack1, stack2;
	char store_ch1, store_ch2;
	//读取字符入栈
	//在这里先开始字符计数
	int character_number = 0;
	while (txt1.get(store_ch1)) {
		++character_number;
		stack1.push(store_ch1);
	}
	while (txt2.get(store_ch2))  stack2.push(store_ch2);

	//去除末尾的的NULL
	if ((!stack1.empty())) {
		stack1.pop();
	}
	if ((!stack2.empty())) {
		stack2.pop();
	}
	//去除文末空行和行末回车
	if ((!stack1.empty())) {
		while (isspace(stack1.top())) {
			stack1.pop();
			if (stack1.empty())  break;
		}
	}
	if ((!stack2.empty())) {
		while (isspace(stack2.top())) {
			stack2.pop();
			if (stack2.empty())  break;
		}
	}

	//flag为1是表明是行末空格，为0时表明是行中空格不用删去
	int flag1 = 1;
	int flag2 = 1;

	while (!stack1.empty() && !stack2.empty()) {
		//判断是否为空栈，是否是行末空格或者制表符
		//小心行首为空行
		while (flag1) {
			if (!stack1.empty()) {
				if ((stack1.top() == ' ') || (stack1.top() == '\t')) {
					stack1.pop();
				}
				else break;
			}
			else break;
		}
		while (flag2) {
			if (!stack2.empty()) {
				if ((stack2.top() == ' ') || (stack2.top() == '\t')) {
					stack2.pop();
				}
				else break;
			}
			else break;
		}
		//考虑这种可能，第一行为空行，所以需要再次判断是否非空
		if (!stack1.empty() && !stack2.empty()) {
			//进行字符比对
			if ((stack1.top()) != (stack2.top())) {
				cerr << "error " << "stack1:" << stack1.top() << "stack2:" << stack2.top() << endl;
				return 1;
			}
			else {
				//遇到字符则说明可以不用判断行末空格了
				if (isalnum(stack1.top())) { flag1 = 0; }
				if (isalnum(stack2.top())) { flag2 = 0; }
				stack1.pop(); stack2.pop();
			}
		}
		else break;


		//表明进入新的一行flag归1，又要开始判断行末空格
		if (!stack1.empty() && !stack2.empty()) {
			if ((stack1.top() == '\n') || (stack1.top() == '\r') || (stack1.top() == '\r\n')) {
				flag1 = 1;
				stack1.pop();
			}
			if ((stack2.top() == '\n') || (stack2.top() == '\r') || (stack2.top() == '\r\n')) {
				flag2 = 1;
				stack2.pop();
			}
		}
		else break;
	}
	//字符数对不上
	if ((!stack1.empty() && stack2.empty()) || (!stack2.empty() && stack1.empty())) {
		cerr << "error";
		return 1;
	}
	//文件大小和字符数
	//变量  行数，字节数，字符数
	int byte_size = 0;
	FILE* fp1 = NULL;
	int file_judge = fopen_s(&fp1, argv[1], "r");
	std::fseek(fp1, 0L, SEEK_END);
	byte_size = ftell(fp1);


	for (int i = 1; i < argc; i++) {
		//输出文件比较信息：：文件路径，文件大小文件行数，文件字符数，比较总用时
		std::cout << "File Info-" << i << endl;
		std::cout << "Path:" << argv[i] << endl;
		std::cout << "Size:" << byte_size << "Byte(s),";
		std::cout << character_number << "character(s)" << endl;
	}


	//输出花费时间：精度为微秒
	auto end = high_resolution_clock::now();
	auto duration = duration_cast<milliseconds>(end - start);   // duration_cast<> 表示类型转换为毫秒
	std::cout << "It takes" << double(duration.count()) << "millisecond(s) to compare the two files" << endl;

	txt1.clear();
	txt2.clear();
	txt1.close();
	txt2.close();
	std::fclose(fp1);
	return 0;
}
