using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Runtime.Caching;
using System.Configuration;

using AdminSolicitudesContratistas.Models.GesintelModel;

namespace AdminSolicitudesContratistas.Models.GesintelMethods
{
    public class Token
    {
        private const string CacheKey = "GesintelCacheToken";

        static int staticCacheTokenMinutosExpiracion = System.Convert.ToInt32(ConfigurationManager.AppSettings.Get("CacheTokenMinutosExpiracion"));
        static int staticGesintelRetryPolicy = System.Convert.ToInt32(ConfigurationManager.AppSettings.Get("GesintelRetryPolicy"));
        static string staticGesintelUser = ConfigurationManager.AppSettings.Get("GesintelUser");
        static string staticGesintelPassword = ConfigurationManager.AppSettings.Get("GesintelPassword");



        //public static AuthenticateResponse gesintelLoginToken()
        //{
        //    try
        //    {
        //        AuthenticateResponse token = new AuthenticateResponse();
        //        int CacheTokenMinutosExpiracion = System.Convert.ToInt32(ConfigurationManager.AppSettings.Get("CacheTokenMinutosExpiracion"));



        //        Log.EscribeLogEventViewer(System.Diagnostics.TraceLevel.Info, DateTime.Now.ToString() + " gesintelLoginToken se desea obtener token");
        //        Token tkn = new Token();
        //        //string token = tkn.getToken();
        //        token = tkn.getToken();
        //        Log.EscribeLogEventViewer(System.Diagnostics.TraceLevel.Info, DateTime.Now.ToString() + " gesintelLoginToken " + token);
        //        if (token.Status != "OK")
        //        {
        //            Log.EscribeLogEventViewer(System.Diagnostics.TraceLevel.Error, "gesintelLoginToken " + "Token vacío");
        //            throw new ArgumentException("Token vacío");
        //        }

        //        //CacheItemPolicy cacheItemPolicy = new CacheItemPolicy
        //        //{
        //        //    AbsoluteExpiration = DateTime.Now.AddMinutes(CacheTokenMinutosExpiracion)
        //        //};
        //        //cache.Add(CacheKey, token, cacheItemPolicy);
        //        Log.EscribeLogEventViewer(System.Diagnostics.TraceLevel.Info, DateTime.Now.ToString() + " gesintelLoginToken nuevo token");
        //        return token;

        //    }
        //    catch (Exception ex)
        //    {
        //        Log.EscribeLogEventViewer(System.Diagnostics.TraceLevel.Error, "gesintelLoginToken " + ex.Message);
        //        throw;
        //    }
        //}

        public static AuthenticateResponse gesintelLoginToken()
        {
            string CacheKey = "Gesintel_Token_Global";
            ObjectCache cache = MemoryCache.Default;

            // 1. Intentar obtener el token del Cache
            AuthenticateResponse token = cache.Get(CacheKey) as AuthenticateResponse;

            if (token != null)
            {
                // FuncionesGenerales.LogTraceProduccion("Usando Token desde CACHE");
                return token;
            }

            // 2. Si no está en cache, pedir uno nuevo de forma sincronizada (lock)
            lock ("lock_token")
            {
                // Doble check para evitar que hilos simultáneos pidan el token al mismo tiempo
                token = cache.Get(CacheKey) as AuthenticateResponse;
                if (token != null) return token;

                try
                {
                    FuncionesGenerales.LogTraceProduccion("Solicitando nuevo TOKEN a API (No estaba en cache)");
                    Token tkn = new Token();
                    token = tkn.getToken();

                    if (token != null && token.Status == "OK" && !string.IsNullOrEmpty(token.Token))
                    {
                        // Guardar en cache por 20 minutos (o lo que dure el token)
                        int minutosExpiracion = 20;
                        CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(minutosExpiracion) };
                        cache.Add(CacheKey, token, policy);

                        FuncionesGenerales.LogTraceProduccion("Nuevo Token obtenido y guardado en CACHE");
                        return token;
                    }

                    throw new Exception("La API devolvió un token inválido o nulo");
                }
                catch (Exception ex)
                {
                    FuncionesGenerales.LogErrorProduccion("gesintelLoginToken", 0, ex);                    
                    throw;
                }
            }
        }





        //public static string gesintelLoginToken()
        //{
        //    try
        //    {
        //        int CacheTokenMinutosExpiracion = staticCacheTokenMinutosExpiracion;

        //        ObjectCache cache = MemoryCache.Default;
        //        if (cache.Contains(CacheKey))
        //        {
        //            GesintelMethods.Log.EscribeLogEventViewer(System.Diagnostics.TraceLevel.Info, DateTime.Now.ToString() + " gesintelLoginToken uso de token en cache");
        //            return (string)cache.Get(CacheKey);
        //        }
        //        else
        //        {
        //            GesintelMethods.Log.EscribeLogEventViewer(System.Diagnostics.TraceLevel.Info, DateTime.Now.ToString() + " gesintelLoginToken se desea obtener token");
        //            Token tkn = new Token();
        //            string token = tkn.getToken();
        //            GesintelMethods.Log.EscribeLogEventViewer(System.Diagnostics.TraceLevel.Info, DateTime.Now.ToString() + " gesintelLoginToken " + token);
        //            if (token == "")
        //            {
        //                GesintelMethods.Log.EscribeLogEventViewer(System.Diagnostics.TraceLevel.Error, "gesintelLoginToken " + "Token vacío");
        //                throw new ArgumentException("Token vacío");
        //            }

        //            CacheItemPolicy cacheItemPolicy = new CacheItemPolicy
        //            {
        //                AbsoluteExpiration = DateTime.Now.AddMinutes(CacheTokenMinutosExpiracion)
        //            };
        //            cache.Add(CacheKey, token, cacheItemPolicy);
        //            GesintelMethods.Log.EscribeLogEventViewer(System.Diagnostics.TraceLevel.Info, DateTime.Now.ToString() + " gesintelLoginToken nuevo token");
        //            return token;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        GesintelMethods.Log.EscribeLogEventViewer(System.Diagnostics.TraceLevel.Error, "gesintelLoginToken " + ex.Message);
        //        throw;
        //    }
        //}


        //private AuthenticateResponse getToken()
        //{
        //    try
        //    {
        //        APIMethods api = new APIMethods();
        //        AuthenticatePayload ap = new AuthenticatePayload
        //        {
        //            Username = ConfigurationManager.AppSettings.Get("GesintelUser"),
        //            Password = ConfigurationManager.AppSettings.Get("GesintelPassword")
        //        };
        //        AuthenticateResponse ar = new AuthenticateResponse();

        //        for (int Index = 1; Index <= System.Convert.ToInt32(ConfigurationManager.AppSettings.Get("GesintelRetryPolicy")); Index++)
        //        {
        //            ar = api.Authenticate(ap);
        //            if (ar.Status.ToUpper() == "OK")
        //                return ar;
        //        }

        //        return ar;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.EscribeLogEventViewer(System.Diagnostics.TraceLevel.Error, "getToken " + ex.Message + ex.InnerException.Message);
        //        throw;
        //    }
        //}

        private AuthenticateResponse getToken()
        {
            try
            {
                APIMethods api = new APIMethods();
                AuthenticatePayload ap = new AuthenticatePayload
                {
                    Username = ConfigurationManager.AppSettings.Get("GesintelUser"),
                    Password = ConfigurationManager.AppSettings.Get("GesintelPassword")
                };

                AuthenticateResponse ar = new AuthenticateResponse();
                int retries = 1;
                try
                {
                    retries = Convert.ToInt32(ConfigurationManager.AppSettings.Get("GesintelRetryPolicy") ?? "1");
                }
                catch { }

                for (int index = 1; index <= retries; index++)
                {
                    ar = api.Authenticate(ap);

                    // Si ar es null, el método Authenticate de APIMethods ya devolvió un objeto con Status="ERROR"
                    if (ar != null && ar.Status != null && ar.Status.ToUpper() == "OK")
                        return ar;

                    FuncionesGenerales.LogTraceProduccion($"TOKEN: Intento {index} de {retries} fallido.");
                }

                return ar;
            }
            catch (Exception ex)
            {
                // Log en TXT (Seguro) - Aquí eliminamos el acceso directo a InnerException.Message
                FuncionesGenerales.LogErrorProduccion("getToken_Private", 0, ex);
                throw;
            }
        }

    }
}