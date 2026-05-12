using ApiDescargaSriV9.Dto;
using ApiDescargaSriV9.Helpers;
using ApiDescargaSriV9.Services;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Formatting = Newtonsoft.Json.Formatting;

namespace ApiDescargaSriV9.CDescarga
{
    public static class Extensions
    {
        public static string RemoveWhiteSpaces(this string str)
        {
            return Regex.Replace(str, @"\s+", String.Empty);
        }
    }
    public class CDescarga
    {
        /// <summary>Retorno de <see cref="CComprobantesElectrnicosRecibidos"/> cuando no hay comprobantes para el filtro en el SRI.</summary>
        public const string ResultadoRecibidosSinDatosSincronizados = "__SRI_RECIBIDOS_SIN_DATOS__";

        int NumeroFilasdata = 0;
        int NumeroFilas = 0;

        private readonly IWebHostEnvironment env;
        private readonly ILogger<CDescarga> _logger;

        public CDescarga(IWebHostEnvironment env, ILogger<CDescarga> logger)
        {
            this.env = env;
            _logger = logger;
        }

        private static ChromeDriverService CreateDriverService()
        {
            var dir = OperatingSystem.IsWindows() ? AppContext.BaseDirectory : "/usr/local/bin";
            var svc = ChromeDriverService.CreateDefaultService(dir);
            svc.HideCommandPromptWindow = true;
            return svc;
        }

        private static void StartBrowserAutoCloseTimer(IWebDriver webDriver, int minutes = 4)
        {
            var autoCloseThread = new Thread(() =>
            {
                Thread.Sleep(TimeSpan.FromMinutes(minutes));
                try
                {
                    webDriver.Quit();
                }
                catch
                {
                    // Si el navegador ya se cerro en otro flujo, ignorar.
                }
            });

            autoCloseThread.IsBackground = true;
            autoCloseThread.Start();
        }

        private string BuildChromeProfileDirectory(string flow, string? userKey)
        {
            var rawUser = (userKey ?? string.Empty).Trim();
            var safeUser = new string(rawUser.Where(char.IsLetterOrDigit).ToArray());
            if (string.IsNullOrWhiteSpace(safeUser))
            {
                safeUser = "default";
            }

            var profilePath = Path.Combine(env.WebRootPath, "ChromeProfiles", flow, safeUser);
            if (!Directory.Exists(profilePath))
            {
                Directory.CreateDirectory(profilePath);
            }

            // Eliminar lock files que quedan cuando Chrome crashea, para que la próxima
            // instancia pueda arrancar con el mismo perfil sin error SessionNotCreated.
            foreach (var lockFile in new[] { "SingletonLock", "SingletonSocket", "SingletonCookie" })
            {
                var path = Path.Combine(profilePath, lockFile);
                if (File.Exists(path)) try { File.Delete(path); } catch { /* ignorar */ }
            }

            return profilePath;
        }

        private void ConfigureDownloadChromeOptions(ChromeOptions options, string downloadDirectory, string flow, string? userKey)
        {
            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("download.directory_upgrade", true);
            options.AddUserProfilePreference("safebrowsing.enabled", true);
            options.AddUserProfilePreference("safebrowsing.enhanced", true);
            options.AddUserProfilePreference("download.default_directory", downloadDirectory);

            var profileDirectory = BuildChromeProfileDirectory(flow, userKey);
            options.AddArgument($"--user-data-dir={profileDirectory}");
            options.AddArgument("--profile-directory=Default");
            options.AddArgument("--no-first-run");
            options.AddArgument("--no-default-browser-check");

            // Flags obligatorios para correr Chrome dentro de contenedor Docker/Linux
            if (!OperatingSystem.IsWindows())
            {
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--window-size=1920,1080");
                options.AddArgument("--start-maximized");
                options.AddArgument("--disable-extensions-except=");
                options.AddArgument("--disable-software-rasterizer");
                options.AddArgument("--remote-debugging-port=0");
            }
        }

        string apiKey = "3885f9c72a38b519d36f5b312e9104c9";
        public string ProcessTRecibidosTabla(IWebDriver driver, IWebElement table)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            Thread.Sleep(100);
            var Recibidos_dat = table.FindElement(By.Id("frmPrincipal:tablaCompRecibidos_data"));

            Thread.Sleep(100);
            IList<IWebElement> ListOfElementsdata = Recibidos_dat.FindElements(By.TagName("tr"));


            List<string> ListaDatosRecibidosTotal = new List<string>();



            // Recorrer las filas y obtener los datos de cada celda
            foreach (IWebElement row in ListOfElementsdata)
            {
                // Obtener todas las celdas de la fila
                IList<IWebElement> cellsth = row.FindElements(By.TagName("th"));
                if (cellsth != null)
                {
                    foreach (IWebElement cell in cellsth)
                    {
                        string cellText = cell.Text; // Obtener el texto dentro de la celda
                    }

                }
                IList<IWebElement> cellstd = row.FindElements(By.TagName("td"));
                if (cellstd != null)
                {
                    foreach (IWebElement cell in cellstd)
                    {
                        ListaDatosRecibidosTotal.Add(cell.Text); // Obtener el texto dentro de la celda
                    }

                }
                // Recorrer las celdas y obtener el texto de cada una

            }




            ListaDatosRecibidosTotal.RemoveAll(string.IsNullOrEmpty);
            var jsonq1 = JsonConvert.SerializeObject(ListaDatosRecibidosTotal);


            Thread.Sleep(2000);

            return jsonq1;


        }
        public List<string> CComprobantesElectrnicosRecibidosTablas(SriDatosRecibidosDto recibidosDto)
        {

            ChromeOptions options = new ChromeOptions();




      //      options.AddExtension(@"wwwroot\\2cap\\RECA.crx");

            IWebDriver webDriver = new ChromeDriver(CreateDriverService(), options);
            StartBrowserAutoCloseTimer(webDriver);




            if (OperatingSystem.IsWindows()) webDriver.Manage().Window.Maximize();

            Thread.Sleep(200);

            IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)webDriver;



            string ventanaPrincipal = webDriver.WindowHandles.FirstOrDefault();
            webDriver.SwitchTo().Window(ventanaPrincipal);
            webDriver.Navigate().GoToUrl("chrome-extension://ifibfemgeogfhoebkmokieepdoobkbpo/options/options.html");
            webDriver.FindElement(By.XPath("/html/body/div/div[1]/table/tbody/tr[1]/td[2]/input")).SendKeys(apiKey);




            var autoSolveRecaptchaV2 = webDriver.FindElement(By.Id("autoSolveRecaptchaV2"));
            Thread.Sleep(200);
            autoSolveRecaptchaV2.Click();
            Thread.Sleep(200);




            Thread.Sleep(200);
            var autoSolveInvisibleRecaptchaV2 = webDriver.FindElement(By.Id("autoSolveInvisibleRecaptchaV2"));
            autoSolveInvisibleRecaptchaV2.Click();
            Thread.Sleep(200);
            var buton2Captcha = webDriver.FindElement(By.Id("connect"));
            buton2Captcha.Click();
            WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));
            IAlert alert = wait.Until(ExpectedConditions.AlertIsPresent());

            alert.Accept();

            Thread.Sleep(2000);
            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/auth/realms/Internet/protocol/openid-connect/auth?client_id=app-sri-claves-angular&redirect_uri=https%3A%2F%2Fsrienlinea.sri.gob.ec%2Fsri-en-linea%2F%2Fcontribuyente%2Fperfil&state=b4595c20-a2ef-45b9-9a2b-77de9fe36b58&nonce=4ba3a978-0bd7-4d22-a2a7-9cd42f0e58e1&response_mode=fragment&response_type=code&scope=openid");

            Thread.Sleep(200);
            webDriver.FindElement(By.Id("usuario")).SendKeys(recibidosDto.Usuario);
            Thread.Sleep(200);
            webDriver.FindElement(By.Id("password")).SendKeys(recibidosDto.Password ?? "");
            Thread.Sleep(200);
            webDriver.FindElement(By.Id("ciAdicional")).SendKeys("");


            Thread.Sleep(200);
            webDriver.FindElement(By.Id("kc-login")).Submit();
            Thread.Sleep(200);



            Thread.Sleep(3000);
            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");

            string ConsultaTablas = "";

            string genero = "NO";
            do
            {
                try
                {
                    try
                    {

                        Thread.Sleep(999);
                        var selectanio = new SelectElement(webDriver.FindElement(By.Id("frmPrincipal:ano")));
                        selectanio.SelectByValue(recibidosDto.Anio);
                        Thread.Sleep(100);
                        var selectmes = new SelectElement(webDriver.FindElement(By.Id("frmPrincipal:mes")));
                        selectmes.SelectByValue(recibidosDto.Mes.ToString());
                        Thread.Sleep(100);
                        var selectdia = new SelectElement(webDriver.FindElement(By.Id("frmPrincipal:dia")));
                        selectdia.SelectByValue(recibidosDto.Dia.ToString());
                        Thread.Sleep(100);
                        var selectComprobante = new SelectElement(webDriver.FindElement(By.Id("frmPrincipal:cmbTipoComprobante")));
                        selectComprobante.SelectByValue(recibidosDto.Comprobante.ToString());
                        Thread.Sleep(100);

                        IWebElement captchalableElement = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
                        .Until(ExpectedConditions.ElementToBeClickable(By.Id("btnRecaptcha")));
                        captchalableElement.Click();
                        Thread.Sleep(200);



                        IWebElement divElement = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
        .Until(ExpectedConditions.ElementExists(By.XPath("//*[@id=\"contenidoPrincipal\"]/div[12]/div[2]/iframe")));
                        if (divElement.Displayed)
                        {
                            genero = "NO";
                            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");

                            continue;



                        }

                        Thread.Sleep(10000);

                        IWebElement dlgpopStatusPrime = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
            .Until(ExpectedConditions.ElementExists(By.Id("dlgpopStatusPrime")));
                        if (dlgpopStatusPrime.Displayed)
                        {
                            genero = "NO";
                            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");

                            continue;
                        }


                        IWebElement tablaCompRecibidos = new WebDriverWait(webDriver, TimeSpan.FromSeconds(25))
 .Until(ExpectedConditions.ElementExists(By.Id("frmPrincipal:tablaCompRecibidos")));

                        var paginator = tablaCompRecibidos.FindElement(By.ClassName("ui-paginator-current")).Text;
                        paginator = paginator.Replace("(", "");
                        paginator = paginator.Replace(")", "");



                        paginator = paginator.Substring(5);
                        List<string> list = new List<string>();
                        for (int i = 1; i <= Convert.ToInt32(paginator); i++)
                        // Loop through each page

                        {

                            // Process the data on the current page
                            ConsultaTablas = ProcessTRecibidosTabla(webDriver, tablaCompRecibidos);
                            list.Add(ConsultaTablas);
                            ConsultaTablas = "";
                            // Click on the next page button
                            ClickNextPage(webDriver);
                        }


                        Thread.Sleep(3000);
                        jsExecutor.ExecuteScript("window.localStorage.clear();");

                        // Clear session storage
                        jsExecutor.ExecuteScript("window.sessionStorage.clear();");

                        // Clear cache
                        webDriver.Manage().Cookies.DeleteAllCookies();
                        Thread.Sleep(100);
                        webDriver.Quit();




                        genero = "SI";

                        return list;

                    }
                    catch (Exception)
                    {
                        IWebElement divElement = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
         .Until(ExpectedConditions.ElementExists(By.XPath("//*[@id=\"contenidoPrincipal\"]/div[12]/div[2]/iframe")));
                        if (divElement.Displayed)
                        {
                            genero = "NO";
                            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");

                            continue;



                        }

                        Thread.Sleep(15000);

                        IWebElement dlgpopStatusPrime = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
    .Until(ExpectedConditions.ElementExists(By.Id("dlgpopStatusPrime")));
                        if (dlgpopStatusPrime.Displayed)
                        {
                            genero = "NO";
                            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");

                            continue;
                        }

                        IWebElement tablaCompRecibidos = new WebDriverWait(webDriver, TimeSpan.FromSeconds(25))
        .Until(ExpectedConditions.ElementExists(By.Id("frmPrincipal:tablaCompRecibidos")));

                        var paginator = tablaCompRecibidos.FindElement(By.ClassName("ui-paginator-current")).Text;
                        paginator = paginator.Replace("(", "");
                        paginator = paginator.Replace(")", "");



                        paginator = paginator.Substring(5);
                        List<string> list = new List<string>();
                        for (int i = 1; i <= Convert.ToInt32(paginator); i++)
                        // Loop through each page

                        {

                            // Process the data on the current page
                            ConsultaTablas = ProcessTRecibidosTabla(webDriver, tablaCompRecibidos);
                            list.Add(ConsultaTablas);
                            ConsultaTablas = "";
                            // Click on the next page button
                            ClickNextPage(webDriver);
                        }


                        Thread.Sleep(3000);
                        Thread.Sleep(100);
                        jsExecutor.ExecuteScript("window.localStorage.clear();");
                        Thread.Sleep(100);
                        // Clear session storage
                        jsExecutor.ExecuteScript("window.sessionStorage.clear();");

                        // Clear cache   Thread.Sleep(100);
                        Thread.Sleep(100);
                        webDriver.Manage().Cookies.DeleteAllCookies();

                        webDriver.Quit();

                        genero = "SI";

                        return list;
                    }
                }
                catch (Exception)
                {

                    genero = "NO";
                    webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");

                    continue;
                }

            } while (genero == "NO");



            return null;





        }



        public ComprobantesJsonRutaXmlDto 
            
            
            CComprobantesElectrnicosRecibidosTablasXmlJson(SriDatosRecibidosDto recibidosDto, string comprobante)
        {

            ChromeOptions options = new ChromeOptions();
            string directorioArchivoPrincipal = Path.Combine(env.WebRootPath, recibidosDto.Usuario);
            string directorioArchivo = Path.Combine(directorioArchivoPrincipal, "CarpetaXmlRecibidos" + recibidosDto.Anio.ToString() + recibidosDto.Mes.ToString() + comprobante);
            string rutaRecibido = Path.Combine(directorioArchivoPrincipal, "CarpetaXmlRecibidos" + recibidosDto.Anio.ToString() + recibidosDto.Mes.ToString() + comprobante + ".zip");

            if (!Directory.Exists(directorioArchivoPrincipal))
            {
                Directory.CreateDirectory(directorioArchivoPrincipal);
            }
            if (!Directory.Exists(directorioArchivo))
            {
                Directory.CreateDirectory(directorioArchivo);
            }


            ConfigureDownloadChromeOptions(options, directorioArchivo, "RecibidosTablasXmlJson", recibidosDto.Usuario);
          //  options.AddExtension(@"wwwroot\\2cap\\RECA.crx");

            IWebDriver webDriver = new ChromeDriver(CreateDriverService(), options);
            StartBrowserAutoCloseTimer(webDriver);




            if (OperatingSystem.IsWindows()) webDriver.Manage().Window.Maximize();

            Thread.Sleep(200);

            IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)webDriver;




            Thread.Sleep(2000);
            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/auth/realms/Internet/protocol/openid-connect/auth?client_id=app-sri-claves-angular&redirect_uri=https%3A%2F%2Fsrienlinea.sri.gob.ec%2Fsri-en-linea%2F%2Fcontribuyente%2Fperfil&state=b4595c20-a2ef-45b9-9a2b-77de9fe36b58&nonce=4ba3a978-0bd7-4d22-a2a7-9cd42f0e58e1&response_mode=fragment&response_type=code&scope=openid");

            Thread.Sleep(200);
            webDriver.FindElement(By.Id("usuario")).SendKeys(recibidosDto.Usuario);
            Thread.Sleep(200);
            webDriver.FindElement(By.Id("password")).SendKeys(recibidosDto.Password ?? "");
            Thread.Sleep(200);
            webDriver.FindElement(By.Id("ciAdicional")).SendKeys("");


            Thread.Sleep(200);
            webDriver.FindElement(By.Id("kc-login")).Submit();
            Thread.Sleep(200);




            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");
            Thread.Sleep(5000);
            string ConsultaTablas = "";

            string genero = "NO";
            do
            {
                try
                {
                    try
                    {

                        Thread.Sleep(100);
                        var selectanio = new SelectElement(webDriver.FindElement(By.Id("frmPrincipal:ano")));
                        selectanio.SelectByValue(recibidosDto.Anio);
                        Thread.Sleep(100);
                        var selectmes = new SelectElement(webDriver.FindElement(By.Id("frmPrincipal:mes")));
                        selectmes.SelectByValue(recibidosDto.Mes.ToString());
                        Thread.Sleep(100);
                        var selectdia = new SelectElement(webDriver.FindElement(By.Id("frmPrincipal:dia")));
                        selectdia.SelectByValue(recibidosDto.Dia.ToString());
                        Thread.Sleep(100);
                        var selectComprobante = new SelectElement(webDriver.FindElement(By.Id("frmPrincipal:cmbTipoComprobante")));
                        selectComprobante.SelectByValue(recibidosDto.Comprobante.ToString());
                        Thread.Sleep(100);

                        //IWebElement captchalableElement = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
                        //.Until(ExpectedConditions.ElementToBeClickable(By.Id("btnRecaptcha")));
                        //captchalableElement.Click();
                        Thread.Sleep(200);
                        EsperarAjaxPrimeFaces(webDriver, 20);


                        SimularMovimientoHumano(webDriver);
                        ScrollHumano(webDriver);

                        EsperarAjaxPrimeFaces(webDriver, 20);

                        DelayHumano();
                        IWebElement btnBuscar = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
                          .Until(ExpectedConditions.ElementToBeClickable(By.Id("frmPrincipal:btnConsultarSinRe")));
                        // Hover real
                        DelayHumano();
                        HoverElemento(webDriver, btnBuscar);
                        DelayHumano();
                        MoveToAndClick(webDriver, btnBuscar);

                        if (ExisteMensajeCaptchaIncorrectaCss(webDriver, 5))
                        {
                            genero = "NO";
                            continue;
                            // reintento
                        }


                        IWebElement divElement = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
        .Until(ExpectedConditions.ElementExists(By.XPath("//*[@id=\"contenidoPrincipal\"]/div[12]/div[2]/iframe")));
                        if (divElement.Displayed)
                        {
                            genero = "NO";
                            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");

                            continue;



                        }

                        Thread.Sleep(10000);

                        IWebElement dlgpopStatusPrime = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
            .Until(ExpectedConditions.ElementExists(By.Id("dlgpopStatusPrime")));
                        if (dlgpopStatusPrime.Displayed)
                        {
                            genero = "NO";
                            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");

                            continue;
                        }


                        IWebElement tablaCompRecibidos = new WebDriverWait(webDriver, TimeSpan.FromSeconds(25))
 .Until(ExpectedConditions.ElementExists(By.Id("frmPrincipal:tablaCompRecibidos")));

                        var paginator = tablaCompRecibidos.FindElement(By.ClassName("ui-paginator-current")).Text;
                        paginator = paginator.Replace("(", "");
                        paginator = paginator.Replace(")", "");



                        paginator = paginator.Substring(5);
                        List<string> list = new List<string>();
                        for (int i = 1; i <= Convert.ToInt32(paginator); i++)
                        // Loop through each page

                        {

                            // Process the data on the current page
                            ConsultaTablas = ProcessTableDataXmlT(webDriver, tablaCompRecibidos, i);
                            list.Add(ConsultaTablas);
                            ConsultaTablas = "";
                            // Click on the next page button
                            ClickNextPage(webDriver);
                        }


                        Thread.Sleep(3000);
                        jsExecutor.ExecuteScript("window.localStorage.clear();");

                        // Clear session storage
                        jsExecutor.ExecuteScript("window.sessionStorage.clear();");

                        // Clear cache
                        webDriver.Manage().Cookies.DeleteAllCookies();
                        Thread.Sleep(100);
                        webDriver.Quit();


                        List<ViewDatosRecibidosDtoJson> viewDatosRecibidosDtos = new List<ViewDatosRecibidosDtoJson>();
                        string consultag = "";

                        foreach (var item in list)
                        {
                            consultag = item;

                            consultag = consultag.Replace("[", "");
                            consultag = consultag.Replace("]", "");
                            consultag = consultag.Replace("\"", "");

                            consultag = consultag.RemoveWhiteSpaces();
                            List<string> values = consultag.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                            for (int i = 0; i < values.Count; i += 7)
                            {
                                ViewDatosRecibidosDtoJson datosRecibidosDto = new ViewDatosRecibidosDtoJson
                                {
                                    Id = Convert.ToInt32(values[i]),
                                    Nro = values[i],
                                    RUCRazonsocialemisor = values[i + 1],
                                    Tiposeriedecomprobante = values[i + 2],
                                    ClaveAccesoAutorizacion = values[i + 3],
                                    Fechahoradeautorizacion = values[i + 4],
                                    Fechaemision = values[i + 5],
                                    Tipoemision = values[i + 6],

                                };



                                viewDatosRecibidosDtos.Add(datosRecibidosDto);
                            }

                        }


                        DirectoryInfo di = new DirectoryInfo(directorioArchivo);

                        foreach (var fi in di.GetFiles())
                        {
                            string ruta = ""; string nombreNuevo = "";
                            if (fi.Extension.Equals(".xml"))
                            {
                                ruta = fi.FullName;
                                //Con esta instrucción obtienes la ruta donde está el archivo origen
                                string soloRuta = Path.GetDirectoryName(ruta);
                                var FacturaXML = XDocument.Load(ruta);
                                string claveAccesoXML = (from frm in FacturaXML.Descendants("numeroAutorizacion")
                                                         select frm.Value).FirstOrDefault();


                                string xmlCdata = "";

                                var cdataElement = FacturaXML.DescendantNodes().OfType<XCData>().FirstOrDefault();
                                if (cdataElement != null)
                                {
                                    xmlCdata = cdataElement.Value;

                                }
                                var cdataXml = XDocument.Parse(xmlCdata);
                                XNamespace dsNamespace = "http://www.w3.org/2000/09/xmldsig#";

                                // Buscar el nodo <ds:Signature> y eliminarlo
                                cdataXml.Descendants(dsNamespace + "Signature").Remove();
                                // Convert XML to JSON
                                //string json = JsonConvert.SerializeXNode(cdataXml);
                                string json = JsonConvert.SerializeObject(cdataXml);

                                JObject jsonObject = JObject.Parse(json);

                                //dynamic jsonObject = JObject.Parse(json);

                                // Agregar un identificador único a cada nodo


                                var buscarxml = viewDatosRecibidosDtos.FirstOrDefault(x => x.ClaveAccesoAutorizacion.Contains(claveAccesoXML));
                                if (buscarxml != null)
                                {


                                    // Convertir el objeto dinámico de vuelta a JSON
                                    string jsonWithIds = jsonObject.ToString(Formatting.Indented);
                                    var cdatajson = JsonConvert.DeserializeObject<ExpandoObject>(jsonWithIds);
                                    buscarxml.ConprobanteXmlJson = cdatajson;


                                }

                                //Con esta instrucción combinas la ruta de origen con el nuevo nombre de archivo






                            }
                            else
                            {
                                fi.Delete();

                            }



                        }


                        ComprobantesJsonRutaXmlDto comprobantesJsonRuta = new ComprobantesJsonRutaXmlDto();
                        comprobantesJsonRuta.EmpresaRuc = recibidosDto.Usuario;
                        comprobantesJsonRuta.datosRecibidosDtoJsons = viewDatosRecibidosDtos;
                        string carpetaParaComprimir = directorioArchivo;

                        string archivoZip = directorioArchivo + ".zip";


                        ZipFile.CreateFromDirectory(carpetaParaComprimir, archivoZip);
                        genero = "SI";

                        comprobantesJsonRuta.EmpresaRutaLink = archivoZip;
                        return comprobantesJsonRuta;

                    }
                    catch (Exception)
                    {
                        IWebElement divElement = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
         .Until(ExpectedConditions.ElementExists(By.XPath("//*[@id=\"contenidoPrincipal\"]/div[12]/div[2]/iframe")));
                        if (divElement.Displayed)
                        {
                            genero = "NO";
                            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");

                            continue;



                        }

                        Thread.Sleep(15000);

                        IWebElement dlgpopStatusPrime = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
    .Until(ExpectedConditions.ElementExists(By.Id("dlgpopStatusPrime")));
                        if (dlgpopStatusPrime.Displayed)
                        {
                            genero = "NO";
                            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");

                            continue;
                        }

                        IWebElement tablaCompRecibidos = new WebDriverWait(webDriver, TimeSpan.FromSeconds(25))
        .Until(ExpectedConditions.ElementExists(By.Id("frmPrincipal:tablaCompRecibidos")));

                        var paginator = tablaCompRecibidos.FindElement(By.ClassName("ui-paginator-current")).Text;
                        paginator = paginator.Replace("(", "");
                        paginator = paginator.Replace(")", "");



                        paginator = paginator.Substring(5);
                        List<string> list = new List<string>();
                        for (int i = 1; i <= Convert.ToInt32(paginator); i++)
                        // Loop through each page

                        {

                            // Process the data on the current page
                            ConsultaTablas = ProcessTableDataXmlT(webDriver, tablaCompRecibidos, i);
                            list.Add(ConsultaTablas);
                            ConsultaTablas = "";
                            // Click on the next page button
                            ClickNextPage(webDriver);
                        }


                        Thread.Sleep(3000);
                        Thread.Sleep(100);
                        jsExecutor.ExecuteScript("window.localStorage.clear();");
                        Thread.Sleep(100);
                        // Clear session storage
                        jsExecutor.ExecuteScript("window.sessionStorage.clear();");

                        // Clear cache   Thread.Sleep(100);
                        Thread.Sleep(100);
                        webDriver.Manage().Cookies.DeleteAllCookies();

                        webDriver.Quit();
                        List<ViewDatosRecibidosDtoJson> viewDatosRecibidosDtos = new List<ViewDatosRecibidosDtoJson>();
                        string consultag = "";

                        foreach (var item in list)
                        {
                            consultag = item;

                            consultag = consultag.Replace("[", "");
                            consultag = consultag.Replace("]", "");
                            consultag = consultag.Replace("\"", "");

                            consultag = consultag.RemoveWhiteSpaces();
                            List<string> values = consultag.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                            for (int i = 0; i < values.Count; i += 7)
                            {
                                ViewDatosRecibidosDtoJson datosRecibidosDto = new ViewDatosRecibidosDtoJson
                                {
                                    Id = Convert.ToInt32(values[i]),
                                    Nro = values[i],
                                    RUCRazonsocialemisor = values[i + 1],
                                    Tiposeriedecomprobante = values[i + 2],
                                    ClaveAccesoAutorizacion = values[i + 3],
                                    Fechahoradeautorizacion = values[i + 4],
                                    Fechaemision = values[i + 5],
                                    Tipoemision = values[i + 6],

                                };



                                viewDatosRecibidosDtos.Add(datosRecibidosDto);




                            }

                        }



                        DirectoryInfo di = new DirectoryInfo(directorioArchivo);


                        foreach (var fi in di.GetFiles())
                        {
                            string ruta = ""; string nombreNuevo = "";
                            if (fi.Extension.Equals(".xml"))
                            {
                                ruta = fi.FullName;
                                //Con esta instrucción obtienes la ruta donde está el archivo origen
                                string soloRuta = Path.GetDirectoryName(ruta);
                                var FacturaXML = XDocument.Load(ruta);
                                string claveAccesoXML = (from frm in FacturaXML.Descendants("numeroAutorizacion")
                                                         select frm.Value).FirstOrDefault();


                                string xmlCdata = "";

                                var cdataElement = FacturaXML.DescendantNodes().OfType<XCData>().FirstOrDefault();
                                if (cdataElement != null)
                                {
                                    xmlCdata = cdataElement.Value;

                                }
                                var cdataXml = XDocument.Parse(xmlCdata);
                                XNamespace dsNamespace = "http://www.w3.org/2000/09/xmldsig#";

                                // Buscar el nodo <ds:Signature> y eliminarlo
                                cdataXml.Descendants(dsNamespace + "Signature").Remove();
                                // Convert XML to JSON
                                //string json = JsonConvert.SerializeXNode(cdataXml);
                                string json = JsonConvert.SerializeObject(cdataXml);

                                JObject jsonObject = JObject.Parse(json);

                                //dynamic jsonObject = JObject.Parse(json);

                                // Agregar un identificador único a cada nodo


                                var buscarxml = viewDatosRecibidosDtos.FirstOrDefault(x => x.ClaveAccesoAutorizacion.Contains(claveAccesoXML));
                                if (buscarxml != null)
                                {


                                    // Convertir el objeto dinámico de vuelta a JSON
                                    string jsonWithIds = jsonObject.ToString(Formatting.Indented);
                                    var cdatajson = JsonConvert.DeserializeObject<ExpandoObject>(jsonWithIds);
                                    buscarxml.ConprobanteXmlJson = cdatajson;


                                }

                                //Con esta instrucción combinas la ruta de origen con el nuevo nombre de archivo






                            }
                            else
                            {
                                fi.Delete();

                            }



                        }





                        ComprobantesJsonRutaXmlDto comprobantesJsonRuta = new ComprobantesJsonRutaXmlDto();
                        comprobantesJsonRuta.EmpresaRuc = recibidosDto.Usuario;
                        comprobantesJsonRuta.datosRecibidosDtoJsons = viewDatosRecibidosDtos;
                        string carpetaParaComprimir = directorioArchivo;

                        string archivoZip = directorioArchivo + ".zip";


                        ZipFile.CreateFromDirectory(carpetaParaComprimir, archivoZip);
                        genero = "SI";

                        comprobantesJsonRuta.EmpresaRutaLink = archivoZip;
                        return comprobantesJsonRuta;
                    }
                }
                catch (Exception)
                {

                    genero = "NO";
                    webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");

                    continue;
                }

            } while (genero == "NO");



            return null;





        }

        public List<ViewDatosRecibidosDto> CComprobantesElectrnicosRecibidosTablasXml(SriDatosRecibidosDto recibidosDto, string comprobante)
        {

            ChromeOptions options = new ChromeOptions();
            string directorioArchivoPrincipal = Path.Combine(env.WebRootPath, recibidosDto.Usuario);
            string directorioArchivo = Path.Combine(directorioArchivoPrincipal, "CarpetaXmlRecibidos" + recibidosDto.Anio.ToString() + recibidosDto.Mes.ToString() + comprobante);
            string rutaRecibido = Path.Combine(directorioArchivoPrincipal, "CarpetaXmlRecibidos" + recibidosDto.Anio.ToString() + recibidosDto.Mes.ToString() + comprobante + ".zip");

            if (!Directory.Exists(directorioArchivoPrincipal))
            {
                Directory.CreateDirectory(directorioArchivoPrincipal);
            }
            if (!Directory.Exists(directorioArchivo))
            {
                Directory.CreateDirectory(directorioArchivo);
            }


            ConfigureDownloadChromeOptions(options, directorioArchivo, "RecibidosTablas", recibidosDto.Usuario);
            if (OperatingSystem.IsWindows()) options.AddExtension(@"wwwroot\2cap\RECA.crx");

            IWebDriver webDriver = new ChromeDriver(CreateDriverService(), options);
            StartBrowserAutoCloseTimer(webDriver);




            if (OperatingSystem.IsWindows()) webDriver.Manage().Window.Maximize();

            Thread.Sleep(200);

            IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)webDriver;



            string ventanaPrincipal = webDriver.WindowHandles.FirstOrDefault();
            webDriver.SwitchTo().Window(ventanaPrincipal);
            webDriver.Navigate().GoToUrl("chrome-extension://ifibfemgeogfhoebkmokieepdoobkbpo/options/options.html");
            webDriver.FindElement(By.XPath("/html/body/div/div[1]/table/tbody/tr[1]/td[2]/input")).SendKeys(apiKey);




            var autoSolveRecaptchaV2 = webDriver.FindElement(By.Id("autoSolveRecaptchaV2"));
            Thread.Sleep(200);
            autoSolveRecaptchaV2.Click();
            Thread.Sleep(200);




            Thread.Sleep(200);
            var autoSolveInvisibleRecaptchaV2 = webDriver.FindElement(By.Id("autoSolveInvisibleRecaptchaV2"));
            autoSolveInvisibleRecaptchaV2.Click();
            Thread.Sleep(200);
            var buton2Captcha = webDriver.FindElement(By.Id("connect"));
            buton2Captcha.Click();
            WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));
            IAlert alert = wait.Until(ExpectedConditions.AlertIsPresent());

            alert.Accept();

            Thread.Sleep(2000);
            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/auth/realms/Internet/protocol/openid-connect/auth?client_id=app-sri-claves-angular&redirect_uri=https%3A%2F%2Fsrienlinea.sri.gob.ec%2Fsri-en-linea%2F%2Fcontribuyente%2Fperfil&state=b4595c20-a2ef-45b9-9a2b-77de9fe36b58&nonce=4ba3a978-0bd7-4d22-a2a7-9cd42f0e58e1&response_mode=fragment&response_type=code&scope=openid");

            Thread.Sleep(200);
            webDriver.FindElement(By.Id("usuario")).SendKeys(recibidosDto.Usuario);
            Thread.Sleep(200);
            webDriver.FindElement(By.Id("password")).SendKeys(recibidosDto.Password ?? "");
            Thread.Sleep(200);
            webDriver.FindElement(By.Id("ciAdicional")).SendKeys("");


            Thread.Sleep(200);
            webDriver.FindElement(By.Id("kc-login")).Submit();
            Thread.Sleep(200);




            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");
            Thread.Sleep(5000);
            string ConsultaTablas = "";

            string genero = "NO";
            do
            {
                try
                {
                    try
                    {

                        Thread.Sleep(100);
                        var selectanio = new SelectElement(webDriver.FindElement(By.Id("frmPrincipal:ano")));
                        selectanio.SelectByValue(recibidosDto.Anio);
                        Thread.Sleep(100);
                        var selectmes = new SelectElement(webDriver.FindElement(By.Id("frmPrincipal:mes")));
                        selectmes.SelectByValue(recibidosDto.Mes.ToString());
                        Thread.Sleep(100);
                        var selectdia = new SelectElement(webDriver.FindElement(By.Id("frmPrincipal:dia")));
                        selectdia.SelectByValue(recibidosDto.Dia.ToString());
                        Thread.Sleep(100);
                        var selectComprobante = new SelectElement(webDriver.FindElement(By.Id("frmPrincipal:cmbTipoComprobante")));
                        selectComprobante.SelectByValue(recibidosDto.Comprobante.ToString());
                        Thread.Sleep(100);

                        IWebElement captchalableElement = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
                        .Until(ExpectedConditions.ElementToBeClickable(By.Id("btnRecaptcha")));
                        captchalableElement.Click();
                        Thread.Sleep(200);



                        IWebElement divElement = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
        .Until(ExpectedConditions.ElementExists(By.XPath("//*[@id=\"contenidoPrincipal\"]/div[12]/div[2]/iframe")));
                        if (divElement.Displayed)
                        {
                            genero = "NO";
                            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");

                            continue;



                        }

                        Thread.Sleep(10000);

                        IWebElement dlgpopStatusPrime = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
            .Until(ExpectedConditions.ElementExists(By.Id("dlgpopStatusPrime")));
                        if (dlgpopStatusPrime.Displayed)
                        {
                            genero = "NO";
                            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");

                            continue;
                        }


                        IWebElement tablaCompRecibidos = new WebDriverWait(webDriver, TimeSpan.FromSeconds(25))
 .Until(ExpectedConditions.ElementExists(By.Id("frmPrincipal:tablaCompRecibidos")));

                        var paginator = tablaCompRecibidos.FindElement(By.ClassName("ui-paginator-current")).Text;
                        paginator = paginator.Replace("(", "");
                        paginator = paginator.Replace(")", "");



                        paginator = paginator.Substring(5);
                        List<string> list = new List<string>();
                        for (int i = 1; i <= Convert.ToInt32(paginator); i++)
                        // Loop through each page

                        {

                            // Process the data on the current page
                            ConsultaTablas = ProcessTableDataXmlT(webDriver, tablaCompRecibidos, i);
                            list.Add(ConsultaTablas);
                            ConsultaTablas = "";
                            // Click on the next page button
                            ClickNextPage(webDriver);
                        }


                        Thread.Sleep(3000);
                        jsExecutor.ExecuteScript("window.localStorage.clear();");

                        // Clear session storage
                        jsExecutor.ExecuteScript("window.sessionStorage.clear();");

                        // Clear cache
                        webDriver.Manage().Cookies.DeleteAllCookies();
                        Thread.Sleep(100);
                        webDriver.Quit();


                        List<ViewDatosRecibidosDto> viewDatosRecibidosDtos = new List<ViewDatosRecibidosDto>();
                        string consultag = "";

                        foreach (var item in list)
                        {
                            consultag = item;

                            consultag = consultag.Replace("[", "");
                            consultag = consultag.Replace("]", "");
                            consultag = consultag.Replace("\"", "");

                            consultag = consultag.RemoveWhiteSpaces();
                            List<string> values = consultag.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                            for (int i = 0; i < values.Count; i += 7)
                            {
                                ViewDatosRecibidosDto datosRecibidosDto = new ViewDatosRecibidosDto
                                {
                                    Id = Convert.ToInt32(values[i]),
                                    Nro = values[i],
                                    RUCRazonsocialemisor = values[i + 1],
                                    Tiposeriedecomprobante = values[i + 2],
                                    ClaveAccesoAutorizacion = values[i + 3],
                                    Fechahoradeautorizacion = values[i + 4],
                                    Fechaemision = values[i + 5],
                                    Tipoemision = values[i + 6],

                                };



                                viewDatosRecibidosDtos.Add(datosRecibidosDto);
                            }

                        }


                        DirectoryInfo di = new DirectoryInfo(directorioArchivo);
                        ViewDatosRecibidosDtolist viewDatosRecibidosDtolist = new ViewDatosRecibidosDtolist();

                        List<ViewDatosRecibidosFacturaDto> viewDatosRecibidosFacturaDtos = new List<ViewDatosRecibidosFacturaDto>();
                        foreach (var fi in di.GetFiles())
                        {
                            string ruta = ""; string nombreNuevo = "";
                            if (fi.Extension.Equals(".xml"))
                            {
                                ruta = fi.FullName;
                                //Con esta instrucción obtienes la ruta donde está el archivo origen
                                string soloRuta = Path.GetDirectoryName(ruta);
                                var FacturaXML = XDocument.Load(ruta);
                                string claveAccesoXML = (from frm in FacturaXML.Descendants("numeroAutorizacion")
                                                         select frm.Value).FirstOrDefault();


                                string xmlCdata = "";

                                var cdataElement = FacturaXML.DescendantNodes().OfType<XCData>().FirstOrDefault();
                                if (cdataElement != null)
                                {
                                    xmlCdata = cdataElement.Value;

                                }
                                var cdataXml = XDocument.Parse(xmlCdata);
                                XNamespace dsNamespace = "http://www.w3.org/2000/09/xmldsig#";

                                // Buscar el nodo <ds:Signature> y eliminarlo
                                cdataXml.Descendants(dsNamespace + "Signature").Remove();
                                // Convert XML to JSON
                                //string json = JsonConvert.SerializeXNode(cdataXml);
                                string json = JsonConvert.SerializeXNode(cdataXml);

                                JObject jsonObject = JObject.Parse(json);
                                if (jsonObject.ContainsKey("ds:Signature"))
                                {
                                    JObject signatureObject = (JObject)jsonObject["ds:Signature"];
                                    // Get the list of properties after "ds:Signature" and remove them.
                                    var propertiesToRemove = signatureObject.Properties().ToList();
                                    foreach (var property in propertiesToRemove)
                                    {
                                        property.Remove();
                                    }
                                }
                                //dynamic jsonObject = JObject.Parse(json);

                                // Agregar un identificador único a cada nodo


                                var buscarxml = viewDatosRecibidosDtos.FirstOrDefault(x => x.ClaveAccesoAutorizacion.Contains(claveAccesoXML));
                                if (buscarxml != null)
                                {


                                    // Convertir el objeto dinámico de vuelta a JSON
                                    string jsonWithIds = jsonObject.ToString(Formatting.Indented);
                                    var cdatajson = JsonConvert.DeserializeObject<ExpandoObject>(jsonWithIds);
                                    string jsonString = JsonConvert.SerializeObject(cdatajson);
                                    string factura = cdataXml.ToString();
                                    XmlSerializer serializer = new XmlSerializer(typeof(Factura2));
                                    Factura2 factura1 = new Factura2();
                                    using (StringReader reader = new StringReader(factura))
                                    {
                                        factura1 = (Factura2)serializer.Deserialize(reader);
                                    }
                                    ViewDatosRecibidosFacturaDto viewDatosRecibidosFacturaDto = new ViewDatosRecibidosFacturaDto();
                                    viewDatosRecibidosFacturaDto.Id = buscarxml.Id;
                                    viewDatosRecibidosFacturaDto.ClaveAcceso = buscarxml.ClaveAccesoAutorizacion;

                                    viewDatosRecibidosFacturaDtos.Add(viewDatosRecibidosFacturaDto);





                                    buscarxml.RazonSocial = factura1.InfoTributaria.RazonSocial;
                                    buscarxml.Ruc = factura1.InfoTributaria.Ruc;
                                    buscarxml.ClaveAcceso = factura1.InfoTributaria.ClaveAcceso;
                                    buscarxml.CodDoc = factura1.InfoTributaria.CodDoc;
                                    buscarxml.Estab = factura1.InfoTributaria.Estab;
                                    buscarxml.PtoEmi = factura1.InfoTributaria.PtoEmi;
                                    buscarxml.Secuencial = factura1.InfoTributaria.Secuencial;
                                    buscarxml.FechaEmision = factura1.InfoFactura.FechaEmision;
                                    buscarxml.ContribuyenteEspecial = factura1.InfoFactura.ContribuyenteEspecial;
                                    buscarxml.ObligadoContabilidad = factura1.InfoFactura.ObligadoContabilidad;
                                    buscarxml.RazonSocialComprador = factura1.InfoFactura.RazonSocialComprador;
                                    buscarxml.IdentificacionComprador = factura1.InfoFactura.IdentificacionComprador;
                                    buscarxml.TotalSinImpuestos = factura1.InfoFactura.TotalSinImpuestos;
                                    buscarxml.TotalDescuento = factura1.InfoFactura.TotalDescuento;
                                    buscarxml.Propina = factura1.InfoFactura.Propina;
                                    buscarxml.ImporteTotal = factura1.InfoFactura.ImporteTotal;
                                    TotalConImpuestosDTO totalConImpuestos = new TotalConImpuestosDTO();
                                    List<TotalImpuestoDTO> totalImpuestoDTOs = new List<TotalImpuestoDTO>();
                                    foreach (var item in factura1.InfoFactura.TotalConImpuestos.TotalImpuesto)
                                    {
                                        TotalImpuestoDTO totalImpuestoDTO = new TotalImpuestoDTO();
                                        totalImpuestoDTO.Codigo = item.Codigo;
                                        totalImpuestoDTO.CodigoPorcentaje = item.CodigoPorcentaje;
                                        totalImpuestoDTO.BaseImponible = item.BaseImponible;
                                        totalImpuestoDTO.Tarifa = item.Tarifa;
                                        totalImpuestoDTO.Valor = item.Valor;

                                        totalImpuestoDTOs.Add(totalImpuestoDTO);


                                    }
                                    totalConImpuestos.TotalImpuesto = totalImpuestoDTOs;
                                    buscarxml.TotalConImpuestos = totalConImpuestos;

                                    PagosDTO pagos = new PagosDTO();
                                    List<PagoDto> pagoDtos = new List<PagoDto>();

                                    foreach (var item in factura1.InfoFactura.Pagos.Pago)
                                    {
                                        PagoDto pagoDto = new PagoDto();
                                        pagoDto.FormaPago = item.FormaPago;
                                        pagoDto.Total = item.Total;
                                        pagoDtos.Add(pagoDto);
                                    }
                                    pagos.Pago = pagoDtos;
                                    buscarxml.Pagos = pagos;


                                    nombreNuevo = Path.Combine(soloRuta, claveAccesoXML + ".xml");

                                    File.Move(ruta, nombreNuevo);


                                }

                                //Con esta instrucción combinas la ruta de origen con el nuevo nombre de archivo






                            }
                            else
                            {
                                fi.Delete();

                            }



                        }


                        viewDatosRecibidosDtolist.viewDatosRecibidosFacturaDtos = viewDatosRecibidosFacturaDtos;

                        string carpetaParaComprimir = directorioArchivo;

                        string archivoZip = directorioArchivo + ".zip";


                        ZipFile.CreateFromDirectory(carpetaParaComprimir, archivoZip);
                        genero = "SI";

                        return viewDatosRecibidosDtos;

                    }
                    catch (Exception)
                    {
                        IWebElement divElement = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
         .Until(ExpectedConditions.ElementExists(By.XPath("//*[@id=\"contenidoPrincipal\"]/div[12]/div[2]/iframe")));
                        if (divElement.Displayed)
                        {
                            genero = "NO";
                            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");

                            continue;



                        }

                        Thread.Sleep(15000);

                        IWebElement dlgpopStatusPrime = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
    .Until(ExpectedConditions.ElementExists(By.Id("dlgpopStatusPrime")));
                        if (dlgpopStatusPrime.Displayed)
                        {
                            genero = "NO";
                            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");

                            continue;
                        }

                        IWebElement tablaCompRecibidos = new WebDriverWait(webDriver, TimeSpan.FromSeconds(25))
        .Until(ExpectedConditions.ElementExists(By.Id("frmPrincipal:tablaCompRecibidos")));

                        var paginator = tablaCompRecibidos.FindElement(By.ClassName("ui-paginator-current")).Text;
                        paginator = paginator.Replace("(", "");
                        paginator = paginator.Replace(")", "");



                        paginator = paginator.Substring(5);
                        List<string> list = new List<string>();
                        for (int i = 1; i <= Convert.ToInt32(paginator); i++)
                        // Loop through each page

                        {

                            // Process the data on the current page
                            ConsultaTablas = ProcessTableDataXmlT(webDriver, tablaCompRecibidos, i);
                            list.Add(ConsultaTablas);
                            ConsultaTablas = "";
                            // Click on the next page button
                            ClickNextPage(webDriver);
                        }


                        Thread.Sleep(3000);
                        Thread.Sleep(100);
                        jsExecutor.ExecuteScript("window.localStorage.clear();");
                        Thread.Sleep(100);
                        // Clear session storage
                        jsExecutor.ExecuteScript("window.sessionStorage.clear();");

                        // Clear cache   Thread.Sleep(100);
                        Thread.Sleep(100);
                        webDriver.Manage().Cookies.DeleteAllCookies();

                        webDriver.Quit();
                        List<ViewDatosRecibidosDto> viewDatosRecibidosDtos = new List<ViewDatosRecibidosDto>();
                        string consultag = "";

                        foreach (var item in list)
                        {
                            consultag = item;

                            consultag = consultag.Replace("[", "");
                            consultag = consultag.Replace("]", "");
                            consultag = consultag.Replace("\"", "");

                            consultag = consultag.RemoveWhiteSpaces();
                            List<string> values = consultag.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                            for (int i = 0; i < values.Count; i += 7)
                            {
                                ViewDatosRecibidosDto datosRecibidosDto = new ViewDatosRecibidosDto
                                {
                                    Id = Convert.ToInt32(values[i]),
                                    Nro = values[i],
                                    RUCRazonsocialemisor = values[i + 1],
                                    Tiposeriedecomprobante = values[i + 2],
                                    ClaveAccesoAutorizacion = values[i + 3],
                                    Fechahoradeautorizacion = values[i + 4],
                                    Fechaemision = values[i + 5],
                                    Tipoemision = values[i + 6],

                                };



                                viewDatosRecibidosDtos.Add(datosRecibidosDto);




                            }

                        }



                        DirectoryInfo di = new DirectoryInfo(directorioArchivo);
                        ViewDatosRecibidosDtolist viewDatosRecibidosDtolist = new ViewDatosRecibidosDtolist();

                        List<ViewDatosRecibidosFacturaDto> viewDatosRecibidosFacturaDtos = new List<ViewDatosRecibidosFacturaDto>();
                        foreach (var fi in di.GetFiles())
                        {
                            string ruta = ""; string nombreNuevo = "";
                            if (fi.Extension.Equals(".xml"))
                            {
                                ruta = fi.FullName;
                                //Con esta instrucción obtienes la ruta donde está el archivo origen
                                string soloRuta = Path.GetDirectoryName(ruta);
                                var FacturaXML = XDocument.Load(ruta);
                                string claveAccesoXML = (from frm in FacturaXML.Descendants("numeroAutorizacion")
                                                         select frm.Value).FirstOrDefault();


                                string xmlCdata = "";

                                var cdataElement = FacturaXML.DescendantNodes().OfType<XCData>().FirstOrDefault();
                                if (cdataElement != null)
                                {
                                    xmlCdata = cdataElement.Value;

                                }
                                var cdataXml = XDocument.Parse(xmlCdata);
                                // Convert XML to JSON
                                string json = JsonConvert.SerializeXNode(cdataXml);

                                JObject jsonObject = JObject.Parse(json);
                                if (jsonObject.ContainsKey("ds:Signature"))
                                {
                                    JObject signatureObject = (JObject)jsonObject["ds:Signature"];
                                    // Get the list of properties after "ds:Signature" and remove them.
                                    var propertiesToRemove = signatureObject.Properties().ToList();
                                    foreach (var property in propertiesToRemove)
                                    {
                                        property.Remove();
                                    }
                                }



                                // Agregar un identificador único a cada nodo




                                var buscarxml = viewDatosRecibidosDtos.FirstOrDefault(x => x.ClaveAccesoAutorizacion.Contains(claveAccesoXML));
                                if (buscarxml != null)
                                {


                                    // Convertir el objeto dinámico de vuelta a JSON
                                    string jsonWithIds = jsonObject.ToString(Formatting.Indented);
                                    var cdatajson = JsonConvert.DeserializeObject<ExpandoObject>(jsonWithIds);
                                    string jsonString = JsonConvert.SerializeObject(cdatajson);
                                    string factura = cdataXml.ToString();
                                    XmlSerializer serializer = new XmlSerializer(typeof(Factura2));
                                    Factura2 factura1 = new Factura2();
                                    using (StringReader reader = new StringReader(factura))
                                    {
                                        factura1 = (Factura2)serializer.Deserialize(reader);
                                    }
                                    ViewDatosRecibidosFacturaDto viewDatosRecibidosFacturaDto = new ViewDatosRecibidosFacturaDto();
                                    viewDatosRecibidosFacturaDto.Id = buscarxml.Id;
                                    viewDatosRecibidosFacturaDto.ClaveAcceso = buscarxml.ClaveAccesoAutorizacion;

                                    viewDatosRecibidosFacturaDtos.Add(viewDatosRecibidosFacturaDto);





                                    buscarxml.RazonSocial = factura1.InfoTributaria.RazonSocial;
                                    buscarxml.Ruc = factura1.InfoTributaria.Ruc;
                                    buscarxml.ClaveAcceso = factura1.InfoTributaria.ClaveAcceso;
                                    buscarxml.CodDoc = factura1.InfoTributaria.CodDoc;
                                    buscarxml.Estab = factura1.InfoTributaria.Estab;
                                    buscarxml.PtoEmi = factura1.InfoTributaria.PtoEmi;
                                    buscarxml.Secuencial = factura1.InfoTributaria.Secuencial;
                                    buscarxml.FechaEmision = factura1.InfoFactura.FechaEmision;
                                    buscarxml.ContribuyenteEspecial = factura1.InfoFactura.ContribuyenteEspecial;
                                    buscarxml.ObligadoContabilidad = factura1.InfoFactura.ObligadoContabilidad;
                                    buscarxml.RazonSocialComprador = factura1.InfoFactura.RazonSocialComprador;
                                    buscarxml.IdentificacionComprador = factura1.InfoFactura.IdentificacionComprador;
                                    buscarxml.TotalSinImpuestos = factura1.InfoFactura.TotalSinImpuestos;
                                    buscarxml.TotalDescuento = factura1.InfoFactura.TotalDescuento;
                                    buscarxml.Propina = factura1.InfoFactura.Propina;
                                    buscarxml.ImporteTotal = factura1.InfoFactura.ImporteTotal;
                                    TotalConImpuestosDTO totalConImpuestos = new TotalConImpuestosDTO();
                                    List<TotalImpuestoDTO> totalImpuestoDTOs = new List<TotalImpuestoDTO>();
                                    foreach (var item in factura1.InfoFactura.TotalConImpuestos.TotalImpuesto)
                                    {
                                        TotalImpuestoDTO totalImpuestoDTO = new TotalImpuestoDTO();
                                        totalImpuestoDTO.Codigo = item.Codigo;
                                        totalImpuestoDTO.CodigoPorcentaje = item.CodigoPorcentaje;
                                        totalImpuestoDTO.BaseImponible = item.BaseImponible;
                                        totalImpuestoDTO.Tarifa = item.Tarifa;
                                        totalImpuestoDTO.Valor = item.Valor;

                                        totalImpuestoDTOs.Add(totalImpuestoDTO);


                                    }
                                    totalConImpuestos.TotalImpuesto = totalImpuestoDTOs;
                                    buscarxml.TotalConImpuestos = totalConImpuestos;

                                    PagosDTO pagos = new PagosDTO();
                                    List<PagoDto> pagoDtos = new List<PagoDto>();

                                    foreach (var item in factura1.InfoFactura.Pagos.Pago)
                                    {
                                        PagoDto pagoDto = new PagoDto();
                                        pagoDto.FormaPago = item.FormaPago;
                                        pagoDto.Total = item.Total;
                                        pagoDtos.Add(pagoDto);
                                    }
                                    pagos.Pago = pagoDtos;
                                    buscarxml.Pagos = pagos;


                                    nombreNuevo = Path.Combine(soloRuta, claveAccesoXML + ".xml");

                                    File.Move(ruta, nombreNuevo);
                                }


                                //Con esta instrucción combinas la ruta de origen con el nuevo nombre de archivo






                            }
                            else
                            {
                                fi.Delete();

                            }



                        }


                        viewDatosRecibidosDtolist.viewDatosRecibidosFacturaDtos = viewDatosRecibidosFacturaDtos;

                        string carpetaParaComprimir = directorioArchivo;

                        string archivoZip = directorioArchivo + ".zip";





                        ZipFile.CreateFromDirectory(carpetaParaComprimir, archivoZip);
                        genero = "SI";


                        return viewDatosRecibidosDtos;
                    }
                }
                catch (Exception)
                {

                    genero = "NO";
                    webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");

                    continue;
                }

            } while (genero == "NO");



            return null;





        }
        public string ProcessTableDataXmlT(IWebDriver driver, IWebElement table, int paginador)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            Thread.Sleep(100);
            var Recibidos_dat = table.FindElement(By.Id("frmPrincipal:tablaCompRecibidos_data"));

            Thread.Sleep(100);
            IList<IWebElement> ListOfElementsdata = Recibidos_dat.FindElements(By.TagName("tr"));
            var inicio = paginador * 50;
            inicio = inicio - 50;
            var NumeroFilas = inicio;

            List<string> ListaDatosRecibidosTotal = new List<string>();
            foreach (var row in ListOfElementsdata)
            {

                IList<IWebElement> cellsth = row.FindElements(By.TagName("th"));
                if (cellsth != null)
                {
                    foreach (IWebElement cell in cellsth)
                    {
                        string cellText = cell.Text; // Obtener el texto dentro de la celda
                    }

                }

                IList<IWebElement> cellstd = row.FindElements(By.TagName("td"));
                if (cellstd != null)
                {

                    foreach (IWebElement cell in cellstd)
                    {
                        ListaDatosRecibidosTotal.Add(cell.Text);

                    }

                }

                Thread.Sleep(100);
                var xml = row.FindElement(By.Id("frmPrincipal:tablaCompRecibidos:" + NumeroFilas.ToString() + ":lnkXml"));



                if (xml != null)
                {
                    Thread.Sleep(100);
                    xml.Click();
                    Thread.Sleep(100);
                    NumeroFilas++;
                }
                else
                {
                    NumeroFilas++;
                }


            }


            ListaDatosRecibidosTotal.RemoveAll(string.IsNullOrEmpty);
            var jsonq1 = JsonConvert.SerializeObject(ListaDatosRecibidosTotal);


            Thread.Sleep(100);

            return jsonq1;



        }

        public string
            CComprobantesElectrnicosRecibidos(SriDatosRecibidosDto recibidosDto, string comprobante)
        {
            _logger.LogInformation("[SRI] Inicio CComprobantesElectrnicosRecibidos | Usuario={U} Anio={A} Mes={M} Dia={D} Comprobante={C}",
                recibidosDto.Usuario, recibidosDto.Anio, recibidosDto.Mes, recibidosDto.Dia, comprobante);

            DateTime currentTimePacific = TimeZoneInfo.ConvertTime(DateTime.Now,
                TimeZoneInfo.FindSystemTimeZoneById(OperatingSystem.IsWindows() ? "SA Pacific Standard Time" : "America/Guayaquil"));
            _logger.LogInformation("[SRI] Hora Pacífico: {Hora}", currentTimePacific);

            ChromeOptions options = new ChromeOptions();
            string directorioArchivoPrincipal, directorioArchivo, rutaRecibido;
            if (recibidosDto.Dia > 0)
            {
                directorioArchivoPrincipal = Path.Combine(env.WebRootPath, "Descargas", recibidosDto.Usuario);
                directorioArchivo = Path.Combine(directorioArchivoPrincipal, "CarpetaXmlRecibidos" + recibidosDto.Anio.ToString() + recibidosDto.Mes.ToString() + recibidosDto.Dia.ToString() + comprobante);
                rutaRecibido = Path.Combine(directorioArchivoPrincipal, "CarpetaXmlRecibidos" + recibidosDto.Anio.ToString() + recibidosDto.Mes.ToString() + recibidosDto.Dia.ToString() + comprobante + ".zip");

                if (!Directory.Exists(directorioArchivoPrincipal))
                {
                    Directory.CreateDirectory(directorioArchivoPrincipal);
                }
                if (!Directory.Exists(directorioArchivo))
                {
                    Directory.CreateDirectory(directorioArchivo);
                }

            }
            else
            {
                directorioArchivoPrincipal = Path.Combine(env.WebRootPath, "Descargas", recibidosDto.Usuario);
                directorioArchivo = Path.Combine(directorioArchivoPrincipal, "CarpetaXmlRecibidos" + recibidosDto.Anio.ToString() + recibidosDto.Mes.ToString() + comprobante);
                rutaRecibido = Path.Combine(directorioArchivoPrincipal, "CarpetaXmlRecibidos" + recibidosDto.Anio.ToString() + recibidosDto.Mes.ToString() + comprobante + ".zip");

                if (!Directory.Exists(directorioArchivoPrincipal))
                {
                    Directory.CreateDirectory(directorioArchivoPrincipal);
                }
                if (!Directory.Exists(directorioArchivo))
                {
                    Directory.CreateDirectory(directorioArchivo);
                }

            }

            _logger.LogInformation("[SRI] Directorios | Principal={P} | Archivo={A}", directorioArchivoPrincipal, directorioArchivo);

            ConfigureDownloadChromeOptions(options, directorioArchivo, "Recibidos", recibidosDto.Usuario);

            //// === CHROME EXE (debes subirlo) ===
            //var chromeExe = @"h:\root\home\cemcadi-001\www\site2\chrome\win64\114.0.5735.90\chrome.exe";
            //if (!File.Exists(chromeExe))
            //    throw new FileNotFoundException("No existe chrome.exe en: " + chromeExe);

            //options.BinaryLocation = chromeExe;

            //// Flags recomendados para servidor
            //options.AddArgument("--headless=new");
            //options.AddArgument("--disable-gpu");
            //options.AddArgument("--window-size=1920,1080");
            //options.AddArgument("--no-sandbox");
            //options.AddArgument("--disable-dev-shm-usage");

            //// Perfil (carpeta escribible)
            //var profileDir = @"h:\root\home\cemcadi-001\www\site2\BrowserProfile";
            //Directory.CreateDirectory(profileDir);
            //options.AddArgument($"--user-data-dir={profileDir}");

            //// === CHROMEDRIVER DIR ===
            //var driverDir = @"h:\root\home\cemcadi-001\www\site2\chromedriver\win32\114.0.5735.90";
            //var driverExe = Path.Combine(driverDir, "chromedriver.exe");
            //if (!File.Exists(driverExe))
            //    throw new FileNotFoundException("No existe chromedriver.exe en: " + driverExe);
            var _driverDir = OperatingSystem.IsWindows() ? AppContext.BaseDirectory : "/usr/local/bin";
            _logger.LogInformation("[CHROME] Iniciando ChromeDriver desde {Dir}...", _driverDir);
            IWebDriver webDriver = new ChromeDriver(CreateDriverService(), options);
            _logger.LogInformation("[CHROME] ChromeDriver OK");
            StartBrowserAutoCloseTimer(webDriver);


            //options.AddExtension(@"wwwroot\\2cap\\RECA.crx");





            //var service = ChromeDriverService.CreateDefaultService();
            //service.HostName = "127.0.0.1";   // evita problemas con localhost/IPv6
            //service.HideCommandPromptWindow = true;


            //var network = webDriver.Manage().Network;
            //network.StartMonitoring();
            //var netSb = new StringBuilder();

            //network.NetworkRequestSent += (s, e) =>
            //{
            //    netSb.AppendLine($"[REQ] {e.RequestMethod} {e.RequestUrl}");
            //    foreach (var h in e.RequestHeaders)
            //        netSb.AppendLine($"  {h.Key}: {h.Value}");
            //};

            //network.NetworkResponseReceived += (s, e) =>
            //{
            //    netSb.AppendLine($"[RES] {e.ResponseStatusCode} {e.ResponseUrl} ({e.ResponseResourceType})");
            //    foreach (var h in e.ResponseHeaders)
            //        netSb.AppendLine($"  {h.Key}: {h.Value}");

            //    // OJO: Body puede ser enorme. Úsalo solo si lo necesitas.
            //    if (e.ResponseResourceType == "Document" || e.ResponseResourceType == "XHR" || e.ResponseResourceType == "Fetch")
            //    {
            //        if (!string.IsNullOrEmpty(e.ResponseBody))
            //            netSb.AppendLine(e.ResponseBody);
            //    }
            //};

            // StartMonitoring es async en Selenium .NET

            if (OperatingSystem.IsWindows()) webDriver.Manage().Window.Maximize();

            Thread.Sleep(200);

            IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)webDriver;



            //string ventanaPrincipal = webDriver.WindowHandles.FirstOrDefault();
            //webDriver.SwitchTo().Window(ventanaPrincipal);
            //webDriver.Navigate().GoToUrl("chrome-extension://ifibfemgeogfhoebkmokieepdoobkbpo/options/options.html");
            //webDriver.FindElement(By.XPath("/html/body/div/div[1]/table/tbody/tr[1]/td[2]/input")).SendKeys(apiKey);




            //var autoSolveRecaptchaV2 = webDriver.FindElement(By.Id("autoSolveRecaptchaV2"));
            //Thread.Sleep(200);
            //autoSolveRecaptchaV2.Click();
            //Thread.Sleep(200);


            //service.EnableVerboseLogging = true;
            //service.LogPath = Path.Combine(directorioArchivo, "chromedriver.log");

            //Thread.Sleep(200);
            //var autoSolveInvisibleRecaptchaV2 = webDriver.FindElement(By.Id("autoSolveInvisibleRecaptchaV2"));
            //autoSolveInvisibleRecaptchaV2.Click();
            //Thread.Sleep(200);
            //var buton2Captcha = webDriver.FindElement(By.Id("connect"));
            //buton2Captcha.Click();
            //WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));
            //IAlert alert = wait.Until(ExpectedConditions.AlertIsPresent());

            //alert.Accept();




            _logger.LogInformation("[SRI] Navegando a página de login...");
            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/auth/realms/Internet/protocol/openid-connect/auth?client_id=app-sri-claves-angular&redirect_uri=https%3A%2F%2Fsrienlinea.sri.gob.ec%2Fsri-en-linea%2F%2Fcontribuyente%2Fperfil&state=b4595c20-a2ef-45b9-9a2b-77de9fe36b58&nonce=4ba3a978-0bd7-4d22-a2a7-9cd42f0e58e1&response_mode=fragment&response_type=code&scope=openid");
            _logger.LogInformation("[SRI] URL tras navegar a login: {U}", webDriver.Url);

            // Si el perfil tiene sesión activa el SRI redirige directo al portal,
            // en ese caso no hay formulario de login y lo saltamos.
            bool hayFormularioLogin = false;
            try
            {
                new WebDriverWait(webDriver, TimeSpan.FromSeconds(5))
                    .Until(ExpectedConditions.ElementIsVisible(By.Id("usuario")));
                hayFormularioLogin = true;
            }
            catch (WebDriverTimeoutException)
            {
                _logger.LogInformation("[SRI] Sesión activa en perfil, sin formulario de login. URL={U}", webDriver.Url);
            }

            if (hayFormularioLogin)
            {
                _logger.LogInformation("[SRI] Formulario de login detectado. Ingresando credenciales...");
                webDriver.FindElement(By.Id("usuario")).SendKeys(recibidosDto.Usuario);
                Thread.Sleep(200);
                webDriver.FindElement(By.Id("password")).SendKeys(recibidosDto.Password ?? "");
                Thread.Sleep(200);
                webDriver.FindElement(By.Id("kc-login")).Submit();
                Thread.Sleep(200);
                _logger.LogInformation("[SRI] Login enviado. URL post-login: {U}", webDriver.Url);
                Thread.Sleep(5000);
            }

            Thread.Sleep(5000);
            _logger.LogInformation("[SRI] Navegando al portal de comprobantes recibidos...");
            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");
            _logger.LogInformation("[SRI] Portal cargado. URL: {U}", webDriver.Url);

            string genero = "NO";


            do
            {
                try
                {
                    try
                    {
                        webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");
                        Thread.Sleep(1000);

                        // =========================
                        // 1) Selección de filtros (PRIMERO)
                        // =========================
                        _logger.LogInformation("[SRI] Seleccionando filtros: Año={A} Mes={M} Dia={D} Comprobante={C}",
                            recibidosDto.Anio, recibidosDto.Mes, recibidosDto.Dia, recibidosDto.Comprobante);
                        var selectanio = new SelectElement(webDriver.FindElement(By.Id("frmPrincipal:ano")));
                        selectanio.SelectByValue(recibidosDto.Anio);
                        EsperarAjaxPrimeFaces(webDriver);

                        var selectmes = new SelectElement(webDriver.FindElement(By.Id("frmPrincipal:mes")));
                        selectmes.SelectByValue(recibidosDto.Mes.ToString());
                        EsperarAjaxPrimeFaces(webDriver);

                        var selectdia = new SelectElement(webDriver.FindElement(By.Id("frmPrincipal:dia")));
                        selectdia.SelectByValue(recibidosDto.Dia.ToString());
                        EsperarAjaxPrimeFaces(webDriver);

                        var selectComprobante = new SelectElement(webDriver.FindElement(By.Id("frmPrincipal:cmbTipoComprobante")));
                        selectComprobante.SelectByValue(recibidosDto.Comprobante.ToString());
                        EsperarAjaxPrimeFaces(webDriver);
                        _logger.LogInformation("[SRI] Filtros seleccionados OK");

                        // Diagnóstico: título, frames e iframes presentes
                        _logger.LogInformation("[SRI-DIAG] Título página: {T}", webDriver.Title);
                        _logger.LogInformation("[SRI-DIAG] URL actual: {U}", webDriver.Url);
                        var frames = webDriver.FindElements(By.TagName("iframe"));
                        _logger.LogInformation("[SRI-DIAG] iframes encontrados: {N}", frames.Count);
                        for (int fi = 0; fi < frames.Count; fi++)
                        {
                            var src = frames[fi].GetAttribute("src") ?? "";
                            var id2  = frames[fi].GetAttribute("id")  ?? "";
                            _logger.LogInformation("[SRI-DIAG]   iframe[{I}] id={Id} src={S}", fi, id2, src);
                        }

                        // Verificar si el botón existe en el DOM (aunque no sea clickeable)
                        var btnEnDom = webDriver.FindElements(By.Id("frmPrincipal:btnConsultarSinRe"));
                        _logger.LogInformation("[SRI-DIAG] btnConsultarSinRe en DOM: {N}", btnEnDom.Count);

                        // Verificar si hay captcha visible
                        var hayRecaptcha = webDriver.FindElements(By.CssSelector("iframe[src*='recaptcha']"));
                        _logger.LogInformation("[SRI-DIAG] iframes recaptcha: {N}", hayRecaptcha.Count);

                        EsperarAjaxPrimeFaces(webDriver, 20);
                        SimularMovimientoHumano(webDriver);
                        ScrollHumano(webDriver);
                        EsperarAjaxPrimeFaces(webDriver, 20);

                        _logger.LogInformation("[SRI] Buscando botón Buscar y haciendo click...");
                        DelayHumano();
                        IWebElement btnBuscar = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
                          .Until(ExpectedConditions.ElementToBeClickable(By.Id("frmPrincipal:btnConsultarSinRe")));
                        DelayHumano();
                        HoverElemento(webDriver, btnBuscar);
                        DelayHumano();
                        MoveToAndClick(webDriver, btnBuscar);
                        _logger.LogInformation("[SRI] Click en Buscar ejecutado. URL={U}", webDriver.Url);

                        if (ExisteMensajeCaptchaIncorrectaCss(webDriver, 5))
                        {
                            _logger.LogWarning("[SRI] Captcha incorrecto detectado, reintentando...");
                            genero = "NO";
                            continue;
                        }
                        // Click final


                        //btnBuscar.Click();
                        //btnBuscar.Click();
                        //btnBuscar.Click();
                        // =========================
                        // 4) Si el server respondió "Captcha incorrecta"
                        //    -> NO recargar toda la página
                        //    -> vuelve a resolver captcha y reintenta click
                        // =========================



                        // =========================
                        // 5) Esperar resultados / tabla
                        // =========================
                        // OJO: dlgpopStatusPrime a veces aparece mientras carga
                        var dlgpop = webDriver.FindElements(By.Id("dlgpopStatusPrime")).FirstOrDefault();
                        if (dlgpop != null && dlgpop.Displayed)
                        {
                            // espera a que desaparezca/cargue
                            Thread.Sleep(1500);
                        }

                        _logger.LogInformation("[SRI] Esperando tabla de resultados (máx 30s)...");
                        IWebElement? tablaCompRecibidos = null;
                        var limiteTablaOMensaje = DateTime.UtcNow.AddSeconds(30);
                        while (DateTime.UtcNow < limiteTablaOMensaje)
                        {
                            if (ExisteMensajeSinDatosGlobal(webDriver))
                            {
                                _logger.LogInformation("[SRI] SRI respondió: sin datos para los parámetros ingresados");
                                try { webDriver.Quit(); } catch { /* ignore */ }
                                return ResultadoRecibidosSinDatosSincronizados;
                            }

                            tablaCompRecibidos = webDriver.FindElements(By.Id("frmPrincipal:tablaCompRecibidos")).FirstOrDefault();
                            if (tablaCompRecibidos != null)
                                break;

                            Thread.Sleep(300);
                        }

                        if (tablaCompRecibidos == null)
                        {
                            _logger.LogWarning("[SRI] Tabla no encontrada tras 30s. URL={U}", webDriver.Url);
                            try { webDriver.Quit(); } catch { /* ignore */ }
                            return ResultadoRecibidosSinDatosSincronizados;
                        }

                        _logger.LogInformation("[SRI] Tabla encontrada. Esperando links XML...");
                        EsperarAjaxPrimeFaces(webDriver, 25);
                        if (!EsperarXmlLinksRecibidosOEstadoVacio(webDriver, tablaCompRecibidos, esperaMaxSegundos: 45))
                        {
                            _logger.LogWarning("[SRI] No se encontraron links XML en la tabla (vacía o timeout)");
                            try { webDriver.Quit(); } catch { /* ignore */ }
                            return ResultadoRecibidosSinDatosSincronizados;
                        }

                        // =========================
                        // 6) Procesar paginación
                        // =========================
                        var paginator = tablaCompRecibidos.FindElement(By.ClassName("ui-paginator-current")).Text;
                        paginator = paginator.Replace("(", "").Replace(")", "");
                        paginator = paginator.Substring(5);
                        _logger.LogInformation("[SRI] Páginas a procesar: {P}", paginator);

                        for (int i = 1; i <= Convert.ToInt32(paginator); i++)
                        {
                            _logger.LogInformation("[SRI] Procesando página {I}/{Total}", i, paginator);
                            ProcessTableData(webDriver, tablaCompRecibidos, i);
                            ClickNextPageT(webDriver);
                        }

                        // =========================
                        // 7) Limpieza + zip
                        // =========================
                        _logger.LogInformation("[SRI] Esperando descarga de archivos (3s)...");
                        Thread.Sleep(3000);

                        var dir = Path.GetDirectoryName(directorioArchivo);
                        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                            Directory.CreateDirectory(dir);

                        jsExecutor.ExecuteScript("window.localStorage.clear()");
                        jsExecutor.ExecuteScript("window.sessionStorage.clear();");
                        webDriver.Manage().Cookies.DeleteAllCookies();
                        webDriver.Manage().Network.StartMonitoring();
                        Thread.Sleep(100);
                        _logger.LogInformation("[CHROME] Cerrando navegador...");
                        webDriver.Quit();
                        _logger.LogInformation("[CHROME] Navegador cerrado");

                        DirectoryInfo di = new DirectoryInfo(directorioArchivo);
                        string ruta = ""; string nombreNuevo = "";

                        foreach (var fi in di.GetFiles())
                        {
                            if (fi.Extension.Equals(".xml"))
                            {
                                ruta = fi.FullName;
                                string soloRuta = Path.GetDirectoryName(ruta);
                                var FacturaXML = XDocument.Load(ruta);
                                string claveAccesoXML = (from frm in FacturaXML.Descendants("numeroAutorizacion")
                                                         select frm.Value).FirstOrDefault();
                                nombreNuevo = Path.Combine(soloRuta, claveAccesoXML + ".xml");
                                File.Move(ruta, nombreNuevo);
                            }
                            else
                            {
                                fi.Delete();
                            }
                        }

                        var xmlsGenerados = new DirectoryInfo(directorioArchivo).GetFiles("*.xml").Length;
                        _logger.LogInformation("[SRI] Proceso completado. XMLs en carpeta: {Count} | Ruta: {R}", xmlsGenerados, directorioArchivo);
                        genero = "SI";
                        return directorioArchivo;
                    }
                    catch (Exception ex1)
                    {
                        _logger.LogError(ex1, "[ERROR] Bloque interno: {Msg} | URL={U}", ex1.Message, webDriver.Url);

                        if (ExisteMensajeSinDatosGlobal(webDriver))
                        {
                            _logger.LogInformation("[SRI] Sin datos (detectado en catch)");
                            try { webDriver.Quit(); } catch { /* ignore */ }
                            return ResultadoRecibidosSinDatosSincronizados;
                        }

                        if (HayMensajeCaptchaIncorrecta(webDriver))
                        {
                            _logger.LogWarning("[SRI] Captcha incorrecta en catch. Esperando resolución...");
                            EsperarTokenRecaptcha(webDriver, 120);
                            genero = "NO";
                            continue;
                        }

                        _logger.LogWarning("[SRI] Error general, reintentando desde portal...");
                        genero = "NO";
                        webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");
                        continue;
                    }
                }
                catch (Exception ex2)
                {
                    Console.WriteLine("[ERROR] Bloque externo: " + ex2.Message);

                    if (ExisteMensajeSinDatosGlobal(webDriver))
                    {
                        try { webDriver.Quit(); } catch { /* ignore */ }
                        return ResultadoRecibidosSinDatosSincronizados;
                    }

                    genero = "NO";
                    webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");
                    continue;
                }

            } while (genero == "NO");

            //        do
            //        {
            //            try
            //            {
            //                try
            //                {

            //                    Thread.Sleep(1000);
            //                    var selectanio = new SelectElement(webDriver.FindElement(By.Id("frmPrincipal:ano")));
            //                    selectanio.SelectByValue(recibidosDto.Anio);
            //                    Thread.Sleep(100);
            //                    var selectmes = new SelectElement(webDriver.FindElement(By.Id("frmPrincipal:mes")));
            //                    selectmes.SelectByValue(recibidosDto.Mes.ToString());
            //                    Thread.Sleep(100);
            //                    var selectdia = new SelectElement(webDriver.FindElement(By.Id("frmPrincipal:dia")));
            //                    selectdia.SelectByValue(recibidosDto.Dia.ToString());
            //                    Thread.Sleep(100);
            //                    var selectComprobante = new SelectElement(webDriver.FindElement(By.Id("frmPrincipal:cmbTipoComprobante")));
            //                    selectComprobante.SelectByValue(recibidosDto.Comprobante.ToString());
            //                    Thread.Sleep(100);

            //                    IWebElement captchalableElement = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
            //                    .Until(ExpectedConditions.ElementToBeClickable(By.Id("frmPrincipal:btnConsultarSinRe")));

            //                    Thread.Sleep(1000);
            //                    captchalableElement.Click();


            //                    Thread.Sleep(5000);

            //                    IWebElement divElement = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
            //    .Until(ExpectedConditions.ElementExists(By.XPath("//*[@id=\"contenidoPrincipal\"]/div[12]/div[2]/iframe")));
            //                    if (divElement.Displayed)
            //                    {
            //                        genero = "NO";
            //                        webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");

            //                        continue;



            //                    }


            //                    IWebElement dlgpopStatusPrime = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
            //    .Until(ExpectedConditions.ElementExists(By.Id("dlgpopStatusPrime")));
            //                    if (dlgpopStatusPrime.Displayed)
            //                    {
            //                        Thread.Sleep(15000);
            //                        IWebElement tablaCompRecibidos1 = new WebDriverWait(webDriver, TimeSpan.FromSeconds(15))
            //    .Until(ExpectedConditions.ElementExists(By.Id("frmPrincipal:tablaCompRecibidos")));
            //                        if (!tablaCompRecibidos1.Displayed)
            //                        {

            //                            genero = "NO";
            //                            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");

            //                            continue;
            //                        }

            //                    }






            //                    IWebElement tablaCompRecibidos = new WebDriverWait(webDriver, TimeSpan.FromSeconds(15))
            //    .Until(ExpectedConditions.ElementExists(By.Id("frmPrincipal:tablaCompRecibidos")));

            //                    var paginator = tablaCompRecibidos.FindElement(By.ClassName("ui-paginator-current")).Text;
            //                    paginator = paginator.Replace("(", "");
            //                    paginator = paginator.Replace(")", "");



            //                    paginator = paginator.Substring(5);

            //                    for (int i = 1; i <= Convert.ToInt32(paginator); i++)
            //                    // Loop through each page

            //                    {
            //                        // Process the data on the current page
            //                        ProcessTableData(webDriver, tablaCompRecibidos, i);

            //                        // Click on the next page button
            //                        ClickNextPageT(webDriver);
            //                    }



            //                    Thread.Sleep(3000);
            //                    jsExecutor.ExecuteScript("window.localStorage.clear();");

            //                    // Clear session storage
            //                    jsExecutor.ExecuteScript("window.sessionStorage.clear();");

            //                    // Clear cache
            //                    webDriver.Manage().Cookies.DeleteAllCookies();
            //                    Thread.Sleep(100);
            //                    webDriver.Quit();

            //                    DirectoryInfo di = new DirectoryInfo(directorioArchivo);
            //                    string ruta = ""; string nombreNuevo = "";

            //                    foreach (var fi in di.GetFiles())
            //                    {
            //                        if (fi.Extension.Equals(".xml"))
            //                        {
            //                            ruta = fi.FullName;
            //                            //Con esta instrucción obtienes la ruta donde está el archivo origen
            //                            string soloRuta = Path.GetDirectoryName(ruta);
            //                            var FacturaXML = XDocument.Load(ruta);
            //                            string claveAccesoXML = (from frm in FacturaXML.Descendants("numeroAutorizacion")
            //                                                     select frm.Value).FirstOrDefault();
            //                            //Con esta instrucción combinas la ruta de origen con el nuevo nombre de archivo
            //                            nombreNuevo = Path.Combine(soloRuta, claveAccesoXML + ".xml");

            //                            File.Move(ruta, nombreNuevo);

            //                        }
            //                        else
            //                        {
            //                            fi.Delete();

            //                        }



            //                    }




            //                    string carpetaParaComprimir = directorioArchivo;

            //                    string archivoZip = directorioArchivo + ".zip";


            //                    ZipFile.CreateFromDirectory(carpetaParaComprimir, archivoZip);
            //                    genero = "SI";

            //                    return archivoZip;

            //                }
            //                catch (Exception)
            //                {
            //                    IWebElement divElement = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
            //     .Until(ExpectedConditions.ElementExists(By.XPath("//*[@id=\"contenidoPrincipal\"]/div[12]/div[2]/iframe")));
            //                    if (divElement.Displayed)
            //                    {
            //                        genero = "NO";
            //                        webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");

            //                        continue;



            //                    }

            //                    Thread.Sleep(5000);

            //                    IWebElement dlgpopStatusPrime = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
            //.Until(ExpectedConditions.ElementExists(By.Id("dlgpopStatusPrime")));
            //                    if (dlgpopStatusPrime.Displayed)
            //                    {
            //                        genero = "NO";
            //                        webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");

            //                        continue;
            //                    }


            //                    IWebElement tablaCompRecibidos = new WebDriverWait(webDriver, TimeSpan.FromSeconds(30))
            //    .Until(ExpectedConditions.ElementExists(By.Id("frmPrincipal:tablaCompRecibidos")));


            //                    var paginator = tablaCompRecibidos.FindElement(By.ClassName("ui-paginator-current")).Text;
            //                    paginator = paginator.Replace("(", "");
            //                    paginator = paginator.Replace(")", "");



            //                    paginator = paginator.Substring(5);




            //                    for (int i = 1; i <= Convert.ToInt32(paginator); i++)
            //                    // Loop through each page

            //                    {
            //                        // Process the data on the current page
            //                        ProcessTableData(webDriver, tablaCompRecibidos, i);

            //                        // Click on the next page button
            //                        ClickNextPageT(webDriver);
            //                    }




            //                    Thread.Sleep(3000);
            //                    Thread.Sleep(100);
            //                    jsExecutor.ExecuteScript("window.localStorage.clear();");
            //                    Thread.Sleep(100);
            //                    // Clear session storage
            //                    jsExecutor.ExecuteScript("window.sessionStorage.clear();");

            //                    // Clear cache   Thread.Sleep(100);
            //                    Thread.Sleep(100);
            //                    webDriver.Manage().Cookies.DeleteAllCookies();

            //                    webDriver.Quit();
            //                    DirectoryInfo di = new DirectoryInfo(directorioArchivo);
            //                    string ruta = ""; string nombreNuevo = "";

            //                    foreach (var fi in di.GetFiles())
            //                    {
            //                        if (fi.Extension.Equals(".xml"))
            //                        {
            //                            ruta = fi.FullName;
            //                            //Con esta instrucción obtienes la ruta donde está el archivo origen
            //                            string soloRuta = Path.GetDirectoryName(ruta);
            //                            var FacturaXML = XDocument.Load(ruta);
            //                            string claveAccesoXML = (from frm in FacturaXML.Descendants("numeroAutorizacion")
            //                                                     select frm.Value).FirstOrDefault();
            //                            //Con esta instrucción combinas la ruta de origen con el nuevo nombre de archivo
            //                            nombreNuevo = Path.Combine(soloRuta, claveAccesoXML + ".xml");

            //                            File.Move(ruta, nombreNuevo);

            //                        }
            //                        else
            //                        {
            //                            fi.Delete();

            //                        }



            //                    }




            //                    string carpetaParaComprimir = directorioArchivo;

            //                    string archivoZip = carpetaParaComprimir + ".zip";


            //                    ZipFile.CreateFromDirectory(carpetaParaComprimir, archivoZip);
            //                    genero = "SI";

            //                    return archivoZip;
            //                }
            //            }
            //            catch (Exception)
            //            {

            //                genero = "NO";
            //                webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55");

            //                continue;
            //            }

            //        } while (genero == "NO");



            return "";

        }
        // ================================
        // Helpers PrimeFaces + reCAPTCHA
        // ================================

        private static void EsperarAjaxPrimeFaces(IWebDriver driver, int timeoutSeg = 20)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeg));
            wait.Until(d =>
            {
                try
                {
                    var js = (IJavaScriptExecutor)d;
                    var done = js.ExecuteScript(@"
                if (window.PrimeFaces && PrimeFaces.ajax && PrimeFaces.ajax.Queue) {
                    return PrimeFaces.ajax.Queue.isEmpty();
                }
                return true;
            ");
                    return done is bool b && b;
                }
                catch
                {
                    return true;
                }
            });
        }

        private static string ObtenerTokenRecaptcha(IWebDriver driver)
        {
            try
            {
                var js = (IJavaScriptExecutor)driver;
                // Ojo: SRI puede usar Enterprise, pero normalmente igual llena g-recaptcha-response
                return (js.ExecuteScript(@"
            var el = document.querySelector('textarea[name=""g-recaptcha-response""]');
            return el ? el.value : '';
        ") ?? "").ToString();
            }
            catch { return ""; }
        }

        private static void EsperarTokenRecaptcha(IWebDriver driver, int timeoutSeg = 120)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeg));
            wait.Until(d =>
            {
                var t = ObtenerTokenRecaptcha(d);
                return !string.IsNullOrWhiteSpace(t) && t.Length > 50;
            });
        }

        private static bool HayMensajeCaptchaIncorrecta(IWebDriver driver)
        {
            try
            {
                // "formMessages:messages" en CSS requiere escapar ':'
                var elems = driver.FindElements(By.CssSelector("#formMessages\\:messages .ui-messages-warn-summary"));
                return elems.Any(e => e.Text != null &&
                                      e.Text.Trim().Contains("Captcha incorrecta", StringComparison.OrdinalIgnoreCase));
            }
            catch { return false; }
        }
        public  bool ExisteMensajeCaptchaIncorrectaCss(IWebDriver driver, int timeoutSeg = 3)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeg));
                return wait.Until(d =>
                {
                    var el = d.FindElements(By.CssSelector("#formMessages\\:messages .ui-messages-warn-summary"));
                    return el.Count > 0 &&
                           (el[0].Text ?? "").Trim().Equals("Captcha incorrecta", StringComparison.OrdinalIgnoreCase);
                });
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        private static bool HayIframeRecaptcha(IWebDriver driver)
        {
            // Más estable que XPaths con div[12]
            return driver.FindElements(By.CssSelector("iframe[src*='recaptcha']")).Any()
                || driver.FindElements(By.CssSelector("iframe[title*='reCAPTCHA']")).Any()
                || driver.FindElements(By.CssSelector("iframe[title*='recaptcha']")).Any();
        }
        public static void MoveToAndClick(IWebDriver driver, IWebElement el)
        {
            var actions = new Actions(driver);

            // Offset pequeño para que no siempre sea el centro exacto
            int offsetX = new Random().Next(-5, 6);
            int offsetY = new Random().Next(-5, 6);

            actions
                .MoveToElement(el, offsetX, offsetY)
                .Pause(TimeSpan.FromMilliseconds(new Random().Next(100, 250)))
                .Click()
                .Perform();

          
        }

        private static void SimularMovimientoHumano(IWebDriver driver)
        {
            var actions = new Actions(driver);

            // Movimiento general por la página
            actions.MoveByOffset(100, 50).Pause(TimeSpan.FromMilliseconds(300))
                   .MoveByOffset(-40, 20).Pause(TimeSpan.FromMilliseconds(250))
                   .MoveByOffset(60, -30).Pause(TimeSpan.FromMilliseconds(400))
                   .Perform();
        }
        private static void DelayHumano(int minMs = 300, int maxMs = 900)
        {
            var rnd = new Random();
            Thread.Sleep(rnd.Next(minMs, maxMs));
        }

        private static void ScrollHumano(IWebDriver driver)
        {
            var js = (IJavaScriptExecutor)driver;

            js.ExecuteScript("window.scrollBy(0, 200);");
            Thread.Sleep(400);

            js.ExecuteScript("window.scrollBy(0, -100);");
            Thread.Sleep(300);
        }
        

        private static void HoverElemento(IWebDriver driver, IWebElement element)
        {
            var actions = new Actions(driver);
            actions.MoveToElement(element)
                   .Pause(TimeSpan.FromMilliseconds(500))
                   .Perform();
        }


        public string CComprobantesElectrnicosEmitidos(SriDatosEmitidosDto emitidosDto, string comprobante)
        {

            ChromeOptions options = new ChromeOptions();
            string directorioArchivoPrincipal = Path.Combine(env.WebRootPath, "Descargas", emitidosDto.Usuario);
            string directorioArchivo = Path.Combine(directorioArchivoPrincipal, emitidosDto.Anio + emitidosDto.Mes + comprobante);
            if (!Directory.Exists(directorioArchivoPrincipal))
            {
                Directory.CreateDirectory(directorioArchivoPrincipal);
            }
            if (!Directory.Exists(directorioArchivo))
            {
                Directory.CreateDirectory(directorioArchivo);
            }
            ConfigureDownloadChromeOptions(options, directorioArchivo, "Emitidos", emitidosDto.Usuario);
            if (OperatingSystem.IsWindows()) options.AddExtension(@"wwwroot\2cap\RECA.crx");

            IWebDriver webDriver = new ChromeDriver(CreateDriverService(), options);
            StartBrowserAutoCloseTimer(webDriver);
            if (OperatingSystem.IsWindows()) webDriver.Manage().Window.Maximize();
            IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)webDriver;




            Thread.Sleep(200);
            string ventanaPrincipal = webDriver.WindowHandles.FirstOrDefault();
            webDriver.SwitchTo().Window(ventanaPrincipal);

            webDriver.Navigate().GoToUrl("chrome-extension://ifibfemgeogfhoebkmokieepdoobkbpo/options/options.html");
            webDriver.FindElement(By.XPath("/html/body/div/div[1]/table/tbody/tr[1]/td[2]/input")).SendKeys(apiKey);
            Thread.Sleep(200);

            var autoSolveRecaptchaV2 = webDriver.FindElement(By.Id("autoSolveRecaptchaV2"));
            Thread.Sleep(200);
            autoSolveRecaptchaV2.Click();
            Thread.Sleep(200);
            var autoSolveInvisibleRecaptchaV2 = webDriver.FindElement(By.Id("autoSolveInvisibleRecaptchaV2"));
            autoSolveInvisibleRecaptchaV2.Click();
            Thread.Sleep(200);
            var buton2Captcha = webDriver.FindElement(By.Id("connect"));
            buton2Captcha.Click();
            WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));
            IAlert alert = wait.Until(ExpectedConditions.AlertIsPresent());

            alert.Accept();

            Thread.Sleep(2000);
            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/auth/realms/Internet/protocol/openid-connect/auth?client_id=app-sri-claves-angular&redirect_uri=https%3A%2F%2Fsrienlinea.sri.gob.ec%2Fsri-en-linea%2F%2Fcontribuyente%2Fperfil&state=b4595c20-a2ef-45b9-9a2b-77de9fe36b58&nonce=4ba3a978-0bd7-4d22-a2a7-9cd42f0e58e1&response_mode=fragment&response_type=code&scope=openid");


            Thread.Sleep(2000);
            webDriver.FindElement(By.Id("usuario")).SendKeys(emitidosDto.Usuario);
            Thread.Sleep(2000);
            webDriver.FindElement(By.Id("password")).SendKeys(emitidosDto.Password);
            Thread.Sleep(2000);
            webDriver.FindElement(By.Id("kc-login")).Submit();
            Thread.Sleep(2000);



            Thread.Sleep(5000);
            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=60&idGrupo=58");
            IWebElement consultaDocumento = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
   .Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"consultaDocumentoForm:panelPrincipal\"]/ul/li[2]/a")));
            consultaDocumento.Click();

            Thread.Sleep(100);











            DateTime primerDiaDelMes = new DateTime(Convert.ToInt32(emitidosDto.Anio), emitidosDto.Mes, 1);
            int diasEnElMes = DateTime.DaysInMonth(Convert.ToInt32(emitidosDto.Anio), emitidosDto.Mes);
            List<string> FechaEmision = new List<string>();
            for (int dia = 1; dia <= diasEnElMes; dia++)
            {
                DateTime fecha = new DateTime(Convert.ToInt32(emitidosDto.Anio), emitidosDto.Mes, dia);
                FechaEmision.Add(fecha.ToString("dd/MM/yyyy"));

            }
            List<string> fechanogenerada = new List<string>();
            int contador = 0;
            do
            {
                if (FechaEmision.Count == contador)
                {
                    FechaEmision.Clear();
                    FechaEmision = new List<string>();
                    FechaEmision = fechanogenerada.ToList();
                    contador = 0;
                    fechanogenerada.Clear();
                }

                foreach (var item in FechaEmision)
                {
                    contador++;

                    try
                    {
                        Thread.Sleep(2000);

                        var selectmes = new SelectElement(webDriver.FindElement(By.Id("frmPrincipal:cmbEstadoAutorizacion")));
                        selectmes.SelectByValue(emitidosDto.Estadoautorizacion);
                        Thread.Sleep(100);
                        var selectdia = new SelectElement(webDriver.FindElement(By.Id("frmPrincipal:cmbTipoComprobante")));
                        selectdia.SelectByValue(emitidosDto.Comprobante.ToString());
                        Thread.Sleep(100);
                        var selectComprobante = new SelectElement(webDriver.FindElement(By.Id("frmPrincipal:cmbEstablecimiento")));
                        selectComprobante.SelectByText(emitidosDto.Establecimiento);
                        Thread.Sleep(100);
                        IWebElement calendarFechaDesde_input = webDriver.FindElement(By.Id("frmPrincipal:calendarFechaDesde_input"));
                        IJavaScriptExecutor js = (IJavaScriptExecutor)webDriver;
                        js.ExecuteScript("arguments[0].setAttribute('value', '" + item + "')", calendarFechaDesde_input);
                        Thread.Sleep(100);
                        IWebElement captchalableElement = new WebDriverWait(webDriver, TimeSpan.FromSeconds(5))
                        .Until(ExpectedConditions.ElementToBeClickable(By.Id("btnRecaptcha")));
                        captchalableElement.Click();
                        Thread.Sleep(5000);


                        IWebElement divElement = new WebDriverWait(webDriver, TimeSpan.FromSeconds(5))
    .Until(ExpectedConditions.ElementExists(By.CssSelector("#contenidoPrincipal > div:nth-child(13)")));
                        if (divElement.Displayed)
                        {
                            fechanogenerada.Add(item);
                            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=60&idGrupo=58");
                            IWebElement consultaDocumento1 = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
                   .Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"consultaDocumentoForm:panelPrincipal\"]/ul/li[2]/a")));
                            consultaDocumento1.Click();
                            continue;



                        }



                        IWebElement dlgpopStatusPrime = new WebDriverWait(webDriver, TimeSpan.FromSeconds(5))
    .Until(ExpectedConditions.ElementExists(By.Id("dlgpopStatusPrime")));
                        if (dlgpopStatusPrime.Displayed)
                        {
                            fechanogenerada.Add(item);
                            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=60&idGrupo=58");
                            IWebElement consultaDocumento1 = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
                   .Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"consultaDocumentoForm:panelPrincipal\"]/ul/li[2]/a")));
                            consultaDocumento1.Click();
                            continue;
                        }









                        try
                        {

                            IWebElement tablaCompEmitidos = new WebDriverWait(webDriver, TimeSpan.FromSeconds(25))
    .Until(ExpectedConditions.ElementExists(By.Id("frmPrincipal:tablaCompEmitidos")));

                            var paginator1 = tablaCompEmitidos.FindElement(By.ClassName("ui-paginator-current"));


                            string paginador1 = paginator1.Text;
                            paginador1 = paginador1.Substring(6, 1);

                            for (int i = 1; i <= Convert.ToInt32(paginador1); i++)
                            // Loop through each page

                            {
                                // Process the data on the current page
                                ProcessCompEmitidos_data(webDriver, tablaCompEmitidos, i);

                                // Click on the next page button
                                ClickNextPageEmitidos(webDriver);
                            }
                            webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=60&idGrupo=58");
                            IWebElement consultaDocumento2 = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
                   .Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"consultaDocumentoForm:panelPrincipal\"]/ul/li[2]/a")));
                            consultaDocumento2.Click();
                            continue;

                        }
                        catch (Exception)
                        {
                            try
                            {



                                IWebElement messages = new WebDriverWait(webDriver, TimeSpan.FromSeconds(20))
            .Until(ExpectedConditions.ElementExists(By.ClassName("ui-messages-warn-summary")));


                                if (messages.Displayed)
                                {

                                    IWebElement close = new WebDriverWait(webDriver, TimeSpan.FromSeconds(5))
                     .Until(ExpectedConditions.ElementToBeClickable(By.ClassName("ui-messages-close")));
                                    close.Click();
                                    webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=60&idGrupo=58");
                                    IWebElement consultaDocumento3 = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
                           .Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"consultaDocumentoForm:panelPrincipal\"]/ul/li[2]/a")));
                                    consultaDocumento3.Click();
                                    continue;

                                }
                                else
                                {

                                    IWebElement dlgpopStatusPrime1 = new WebDriverWait(webDriver, TimeSpan.FromSeconds(15))
        .Until(ExpectedConditions.ElementExists(By.Id("dlgpopStatusPrime")));
                                    if (dlgpopStatusPrime1.Displayed)
                                    {
                                        fechanogenerada.Add(item);
                                        webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=60&idGrupo=58");
                                        IWebElement consultaDocumento3 = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
                               .Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"consultaDocumentoForm:panelPrincipal\"]/ul/li[2]/a")));
                                        consultaDocumento3.Click();
                                        continue;
                                    }


                                    IWebElement tablaCompEmitidos = new WebDriverWait(webDriver, TimeSpan.FromSeconds(25))
          .Until(ExpectedConditions.ElementExists(By.Id("frmPrincipal:tablaCompEmitidos")));


                                    var paginator1 = tablaCompEmitidos.FindElement(By.ClassName("ui-paginator-current"));


                                    string paginador1 = paginator1.Text;

                                    paginador1 = paginador1.Replace("(", "");
                                    paginador1 = paginador1.Replace(")", "");
                                    paginador1 = paginador1.Substring(5);
                                    for (int i = 1; i <= Convert.ToInt32(paginador1); i++)
                                    // Loop through each page

                                    {
                                        // Process the data on the current page
                                        ProcessCompEmitidos_data(webDriver, tablaCompEmitidos, i);

                                        // Click on the next page button
                                        ClickNextPageEmitidos(webDriver);
                                    }
                                    webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=60&idGrupo=58");
                                    IWebElement consultaDocumento2 = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
                           .Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"consultaDocumentoForm:panelPrincipal\"]/ul/li[2]/a")));
                                    consultaDocumento2.Click();


                                    continue;
                                }


                            }
                            catch (Exception)
                            {
                                IWebElement dlgpopStatusPrime1 = new WebDriverWait(webDriver, TimeSpan.FromSeconds(15))
    .Until(ExpectedConditions.ElementExists(By.Id("dlgpopStatusPrime")));
                                if (dlgpopStatusPrime1.Displayed)
                                {
                                    fechanogenerada.Add(item);
                                    webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=60&idGrupo=58");
                                    IWebElement consultaDocumento3 = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
                           .Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"consultaDocumentoForm:panelPrincipal\"]/ul/li[2]/a")));
                                    consultaDocumento3.Click();
                                    continue;
                                }


                                IWebElement tablaCompEmitidos = new WebDriverWait(webDriver, TimeSpan.FromSeconds(25))
      .Until(ExpectedConditions.ElementExists(By.Id("frmPrincipal:tablaCompEmitidos")));


                                var paginator1 = tablaCompEmitidos.FindElement(By.ClassName("ui-paginator-current"));
                                string paginador1 = paginator1.Text;

                                paginador1 = paginador1.Replace("(", "");
                                paginador1 = paginador1.Replace(")", "");
                                paginador1 = paginador1.Substring(5);

                                for (int i = 1; i <= Convert.ToInt32(paginador1); i++)
                                // Loop through each page

                                {
                                    // Process the data on the current page
                                    ProcessCompEmitidos_data(webDriver, tablaCompEmitidos, i);

                                    // Click on the next page button
                                    ClickNextPageEmitidos(webDriver);
                                }
                                webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=60&idGrupo=58");
                                IWebElement consultaDocumento2 = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
                       .Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"consultaDocumentoForm:panelPrincipal\"]/ul/li[2]/a")));
                                consultaDocumento2.Click();


                                continue;
                            }


                        }









                    }
                    catch
                    {
                        fechanogenerada.Add(item);
                        webDriver.Navigate().GoToUrl("https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=60&idGrupo=58");
                        IWebElement consultaDocumento1 = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10))
               .Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"consultaDocumentoForm:panelPrincipal\"]/ul/li[2]/a")));
                        consultaDocumento1.Click();
                        continue;



                    }
                }


            } while (fechanogenerada.Count > 0);




            jsExecutor.ExecuteScript("window.localStorage.clear();");

            jsExecutor.ExecuteScript("window.sessionStorage.clear();");


            webDriver.Manage().Cookies.DeleteAllCookies();

            webDriver.Quit();
            DirectoryInfo di = new DirectoryInfo(directorioArchivo);
            string ruta = ""; string nombreNuevo = "";

            foreach (var fi in di.GetFiles())
            {
                if (fi.Extension.Equals(".xml"))
                {
                    ruta = fi.FullName;
                    //Con esta instrucción obtienes la ruta donde está el archivo origen
                    string soloRuta = Path.GetDirectoryName(ruta);
                    var FacturaXML = XDocument.Load(ruta);
                    string claveAccesoXML = (from fr in FacturaXML.Descendants("numeroAutorizacion")
                                             select fr.Value).FirstOrDefault();
                    //Con esta instrucción combinas la ruta de origen con el nuevo nombre de archivo
                    nombreNuevo = Path.Combine(soloRuta, claveAccesoXML + ".xml");

                    File.Move(ruta, nombreNuevo);

                }
                else
                {
                    fi.Delete();

                }



            }




            return directorioArchivo;

        }

        /// <summary>
        /// Tras Buscar en recibidos: espera a que aparezcan enlaces XML (hay datos) o un estado claro de tabla vacía.
        /// Devuelve false si no hay enlaces descargables (sin datos / timeout sin XML).
        /// </summary>
        private static bool EsperarXmlLinksRecibidosOEstadoVacio(IWebDriver webDriver, IWebElement tablaCompRecibidos, int esperaMaxSegundos)
        {
            var limite = DateTime.UtcNow.AddSeconds(esperaMaxSegundos);
            while (DateTime.UtcNow < limite)
            {
                try
                {
                    // Mensaje explícito del portal cuando no existen registros para el filtro.
                    if (ExisteMensajeSinDatosGlobal(webDriver))
                    {
                        return false;
                    }

                    foreach (var el in webDriver.FindElements(By.CssSelector(".ui-messages-warn-summary")))
                    {
                        var txt = (el.Text ?? string.Empty).Trim();
                        if (el.Displayed && txt.Contains("No existen datos", StringComparison.OrdinalIgnoreCase))
                            return false;
                    }

                    foreach (var el in webDriver.FindElements(By.CssSelector(".ui-datatable-empty-message")))
                    {
                        if (el.Displayed)
                            return false;
                    }

                    foreach (var el in webDriver.FindElements(By.XPath("//*[contains(@class,'ui-datatable')]//*[contains(text(),'No existen')]")))
                    {
                        if (el.Displayed)
                            return false;
                    }

                    if (tablaCompRecibidos.FindElements(By.XPath(".//*[contains(@id,'lnkXml')]")).Count > 0)
                        return true;
                }
                catch
                {
                    // sigue esperando
                }

                Thread.Sleep(450);
            }

            try
            {
                return tablaCompRecibidos.FindElements(By.XPath(".//*[contains(@id,'lnkXml')]")).Count > 0;
            }
            catch
            {
                return false;
            }
        }

        private static bool ExisteMensajeSinDatosGlobal(IWebDriver webDriver)
        {
            try
            {
                var body = webDriver.FindElements(By.TagName("body")).FirstOrDefault();
                var bodyText = (body?.Text ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(bodyText))
                    return false;

                return bodyText.Contains("No existen datos para los parámetros", StringComparison.OrdinalIgnoreCase)
                    || bodyText.Contains("No existen datos para los parametros", StringComparison.OrdinalIgnoreCase)
                    || bodyText.Contains("No existen datos", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public void ProcessTableData(IWebDriver driver, IWebElement table, int paginador)
        {
            ScrollDerechaPagina(driver);

            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            Thread.Sleep(100);
            var Recibidos_dat = table.FindElement(By.Id("frmPrincipal:tablaCompRecibidos_data"));

            Thread.Sleep(100);
            IList<IWebElement> ListOfElementsdata = Recibidos_dat.FindElements(By.TagName("tr"));
            var inicio = (paginador * 50) - 50;
            var NumeroFilas = inicio;

            foreach (var row in ListOfElementsdata)
            {
                Thread.Sleep(150);
                var pdf = row.FindElement(By.Id($"frmPrincipal:tablaCompRecibidos:{NumeroFilas}:lnkPdf"));
                var xml = row.FindElement(By.Id($"frmPrincipal:tablaCompRecibidos:{NumeroFilas}:lnkXml"));
                if (xml != null)
                {
                    Thread.Sleep(200);
                    xml.Click();

                    //pdf.Click();
         
                    Thread.Sleep(200);
                }
                NumeroFilas++;
            }
        }

        // Enviar el scroll horizontal de toda la página al máximo a la derecha
        private void ScrollDerechaPagina(IWebDriver driver)
        {
            var js = (IJavaScriptExecutor)driver;
            js.ExecuteScript(@"
        var de = document.documentElement;
        var b = document.body;
        var x = Math.max(
            (de && de.scrollWidth ? de.scrollWidth : 0),
            (b && b.scrollWidth ? b.scrollWidth : 0)
        );
        window.scrollTo(x, 0);
    ");
            Thread.Sleep(200);
        }

        public void ProcessCompEmitidos_data(IWebDriver driver, IWebElement table, int paginador)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            Thread.Sleep(100);
            var tablaCompEmitidos_data = driver.FindElement(By.Id("frmPrincipal:tablaCompEmitidos_data"));

            Thread.Sleep(100);
            IList<IWebElement> ListOfElementsdata = tablaCompEmitidos_data.FindElements(By.TagName("tr"));
            var inicio = paginador * 50;
            inicio = inicio - 50;
            var NumeroFilas = inicio;

            foreach (var row in ListOfElementsdata)
            {

                Thread.Sleep(100);
                var xml = row.FindElement(By.Id("frmPrincipal:tablaCompEmitidos:" + NumeroFilas.ToString() + ":lnkXml"));



                if (xml != null)
                {
                    Thread.Sleep(100);
                    xml.Click();
                    Thread.Sleep(100);
                    NumeroFilas++;
                }
                else
                {
                    NumeroFilas++;
                }


            }




        }

        public void ClickNextPageEmitidos(IWebDriver driver)
        {
            // Find and click the "Next" button in the pagination UI
            IWebElement tablaCompEmitidos_paginator_bottom = new WebDriverWait(driver, TimeSpan.FromSeconds(10))
                      .Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"frmPrincipal:tablaCompEmitidos_paginator_bottom\"]/span[4]")));
            Thread.Sleep(100);
            tablaCompEmitidos_paginator_bottom.Click();
            Thread.Sleep(100);
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, 0)");
            Thread.Sleep(100);
        }
        public void ClickNextPage(IWebDriver driver)
        {
            // Find and click the "Next" button in the pagination UI
            IWebElement tablaCompRecibidos_paginator_bottom = new WebDriverWait(driver, TimeSpan.FromSeconds(10))
                        .Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"frmPrincipal:tablaCompRecibidos_paginator_bottom\"]/span[4]")));
            Thread.Sleep(100);
            tablaCompRecibidos_paginator_bottom.Click();
            Thread.Sleep(100);

            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, 0)");
            Thread.Sleep(100);
        }
        public void ClickNextPageT(IWebDriver driver)
        {
            // Find and click the "Next" button in the pagination UI
            IWebElement tablaCompRecibidos_paginator_bottom = new WebDriverWait(driver, TimeSpan.FromSeconds(10))
                        .Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"frmPrincipal:tablaCompRecibidos_paginator_bottom\"]/span[4]")));
            Thread.Sleep(100);
            tablaCompRecibidos_paginator_bottom.Click();
            Thread.Sleep(100);

            IWebElement tablaCompRecibidos = new WebDriverWait(driver, TimeSpan.FromSeconds(30))
.Until(ExpectedConditions.ElementExists(By.Id("frmPrincipal:tablaCompRecibidos")));


            // Ejecuta un script de JavaScript para desplazarte hasta la posición de la tabla
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", tablaCompRecibidos);



            Thread.Sleep(100);
        }

    }

}
