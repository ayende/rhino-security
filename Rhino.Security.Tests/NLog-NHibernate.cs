using System;
using System.Collections.Specialized;
using System.Configuration;
using NLog;
using NHibernate;

namespace Rhino.Security.Tests
{
	///Add <add key="nhibernate-logger" value="MyNamespace.NLogFactory, MyAssemblyName"/>
    
    public class NLogFactory : ILoggerFactory
    {
        #region ILoggerFactory Members

        public IInternalLogger LoggerFor(Type type)
        {
            return new NLogLogger(LogManager.GetLogger(type.FullName));
        }

        public IInternalLogger LoggerFor(string keyName)
        {
            return new NLogLogger(LogManager.GetLogger(keyName));
        }

        #endregion
    }

    public class NLogLogger : IInternalLogger
    {
        private readonly Logger logger;

        public NLogLogger(Logger logger)
        {
            this.logger = logger;
        }

        #region Properties

        public bool IsDebugEnabled { get { return logger.IsDebugEnabled; } }

        public bool IsErrorEnabled { get { return logger.IsErrorEnabled; } }

        public bool IsFatalEnabled { get { return logger.IsFatalEnabled; } }

        public bool IsInfoEnabled { get { return logger.IsInfoEnabled; } }

        public bool IsWarnEnabled { get { return logger.IsWarnEnabled; } }

        #endregion

        #region IInternalLogger Methods

        public void Debug(object message, Exception exception)
        {
            logger.Debug(message.ToString(), exception);
        }

        public void Debug(object message)
        {
            logger.Debug(message.ToString());
        }

        public void DebugFormat(string format, params object[] args)
        {
            logger.Debug(String.Format(format, args));
        }

        public void Error(object message, Exception exception)
        {
            logger.Error(message.ToString(), exception);
        }

        public void Error(object message)
        {
            logger.Error(message.ToString());
        }

        public void ErrorFormat(string format, params object[] args)
        {
            logger.Error(String.Format(format, args));
        }

        public void Fatal(object message, Exception exception)
        {
            logger.Fatal(message.ToString(), exception);
        }

        public void Fatal(object message)
        {
            logger.Fatal(message.ToString());
        }

        public void Info(object message, Exception exception)
        {
            logger.Info(message.ToString(), exception);
        }

        public void Info(object message)
        {
            logger.Info(message.ToString());
        }

        public void InfoFormat(string format, params object[] args)
        {
            logger.Info(String.Format(format, args));
        }

        public void Warn(object message, Exception exception)
        {
            logger.Warn(message.ToString(), exception);
        }

        public void Warn(object message)
        {
            logger.Warn(message.ToString());
        }

        public void WarnFormat(string format, params object[] args)
        {
            logger.Warn(String.Format(format, args));
        }

        #endregion
    }
}