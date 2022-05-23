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
	//ʹ��chrono �߾��ȼ�ʱ
	auto start = high_resolution_clock::now();

	//�쳣��Ϣ_���θ����������
	if (argc != 3) {
		cerr << "�����и�ʽ����ʹ�ø�ʽ��\n������ �ļ���1 �ļ���2\n";
		return 1;
	}

	ifstream txt1, txt2;
	txt1.open(argv[1]);
	txt2.open(argv[2]);

	//�쳣��Ϣ_�ļ��޷���
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
	//��ȡ�ַ���ջ
	//�������ȿ�ʼ�ַ�����
	int character_number = 0;
	while (txt1.get(store_ch1)) {
		++character_number;
		stack1.push(store_ch1);
	}
	while (txt2.get(store_ch2))  stack2.push(store_ch2);

	//ȥ��ĩβ�ĵ�NULL
	if ((!stack1.empty())) {
		stack1.pop();
	}
	if ((!stack2.empty())) {
		stack2.pop();
	}
	//ȥ����ĩ���к���ĩ�س�
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

	//flagΪ1�Ǳ�������ĩ�ո�Ϊ0ʱ���������пո���ɾȥ
	int flag1 = 1;
	int flag2 = 1;

	while (!stack1.empty() && !stack2.empty()) {
		//�ж��Ƿ�Ϊ��ջ���Ƿ�����ĩ�ո�����Ʊ��
		//С������Ϊ����
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
		//�������ֿ��ܣ���һ��Ϊ���У�������Ҫ�ٴ��ж��Ƿ�ǿ�
		if (!stack1.empty() && !stack2.empty()) {
			//�����ַ��ȶ�
			if ((stack1.top()) != (stack2.top())) {
				cerr << "error " << "stack1:" << stack1.top() << "stack2:" << stack2.top() << endl;
				return 1;
			}
			else {
				//�����ַ���˵�����Բ����ж���ĩ�ո���
				if (isalnum(stack1.top())) { flag1 = 0; }
				if (isalnum(stack2.top())) { flag2 = 0; }
				stack1.pop(); stack2.pop();
			}
		}
		else break;


		//���������µ�һ��flag��1����Ҫ��ʼ�ж���ĩ�ո�
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
	//�ַ����Բ���
	if ((!stack1.empty() && stack2.empty()) || (!stack2.empty() && stack1.empty())) {
		cerr << "error";
		return 1;
	}
	//�ļ���С���ַ���
	//����  �������ֽ������ַ���
	int byte_size = 0;
	FILE* fp1 = NULL;
	int file_judge = fopen_s(&fp1, argv[1], "r");
	std::fseek(fp1, 0L, SEEK_END);
	byte_size = ftell(fp1);


	for (int i = 1; i < argc; i++) {
		//����ļ��Ƚ���Ϣ�����ļ�·�����ļ���С�ļ��������ļ��ַ������Ƚ�����ʱ
		std::cout << "File Info-" << i << endl;
		std::cout << "Path:" << argv[i] << endl;
		std::cout << "Size:" << byte_size << "Byte(s),";
		std::cout << character_number << "character(s)" << endl;
	}


	//�������ʱ�䣺����Ϊ΢��
	auto end = high_resolution_clock::now();
	auto duration = duration_cast<milliseconds>(end - start);   // duration_cast<> ��ʾ����ת��Ϊ����
	std::cout << "It takes" << double(duration.count()) << "millisecond(s) to compare the two files" << endl;

	txt1.clear();
	txt2.clear();
	txt1.close();
	txt2.close();
	std::fclose(fp1);
	return 0;
}
