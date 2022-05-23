using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JudgePlatform.Models
{
	/// <summary>
	/// 评测状态枚举器。
	/// </summary>
	enum JudgeStatus
	{
		/// <summary>
		/// 程序通过。
		/// </summary>
		Accepted,
		/// <summary>
		/// 答案错误。
		/// </summary>
		WrongAnswer,
		/// <summary>
		/// 格式错误。
		/// </summary>
		PresentationError,
		/// <summary>
		/// 编译错误。
		/// </summary>
		CompileError,
		/// <summary>
		/// 运行时错误。
		/// </summary>
		RuntimeError,
		/// <summary>
		/// 运行超时。
		/// </summary>
		TimeLimitExceeded,
		/// <summary>
		/// 内存超限。
		/// </summary>
		MemoryLimitExceeded,
		/// <summary>
		/// 输出超限。
		/// </summary>
		OutputLimitExceeded,
		/// <summary>
		/// 等待评测。
		/// </summary>
		Pending,
		/// <summary>
		/// 正在编译。
		/// </summary>
		Compiling,
		/// <summary>
		/// 正在运行。
		/// </summary>
		Running,
		/// <summary>
		/// 文件异常。
		/// </summary>
		FileException,
		/// <summary>
		/// 编译异常。
		/// </summary>
		CompileException,
		/// <summary>
		/// 数据异常。
		/// </summary>
		TestException,
		/// <summary>
		/// 系统异常。
		/// </summary>
		InternalException,
		/// <summary>
		/// 多种错误。
		/// </summary>
		MultipleError,
	}
}
