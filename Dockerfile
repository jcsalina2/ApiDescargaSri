# Forzar amd64: Google Chrome solo existe para linux/amd64
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ApiDescargaSriV9.csproj .
RUN for i in 1 2 3 4 5; do dotnet restore && break || (echo "Retry $i..." && sleep 10); done

COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# ─── Runtime ───────────────────────────────────────────────────────────────────
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Chrome + Xvfb para Selenium sin display físico
RUN apt-get update && apt-get install -y --no-install-recommends \
        wget gnupg xvfb unzip \
        fonts-liberation libatk-bridge2.0-0 libatk1.0-0 libcairo2 \
        libcups2 libdbus-1-3 libexpat1 libfontconfig1 libgbm1 \
        libglib2.0-0 libgtk-3-0 libnspr4 libnss3 libpango-1.0-0 \
        libpangocairo-1.0-0 libx11-6 libx11-xcb1 libxcb1 libxcomposite1 \
        libxcursor1 libxdamage1 libxext6 libxfixes3 libxi6 libxrandr2 \
        libxrender1 libxss1 libxtst6 lsb-release xdg-utils \
    && wget -qO- https://dl.google.com/linux/linux_signing_key.pub \
        | gpg --dearmor -o /usr/share/keyrings/google-chrome.gpg \
    && echo "deb [arch=amd64 signed-by=/usr/share/keyrings/google-chrome.gpg] \
        http://dl.google.com/linux/chrome/deb/ stable main" \
        > /etc/apt/sources.list.d/google-chrome.list \
    && apt-get update && apt-get install -y --no-install-recommends google-chrome-stable \
    && rm -rf /var/lib/apt/lists/* \
    && CHROME_VER=$(google-chrome-stable --version | grep -oP '\d+\.\d+\.\d+\.\d+') \
    && echo "Downloading chromedriver $CHROME_VER" \
    && wget -q "https://storage.googleapis.com/chrome-for-testing-public/${CHROME_VER}/linux64/chromedriver-linux64.zip" \
         -O /tmp/cd.zip \
    && unzip /tmp/cd.zip chromedriver-linux64/chromedriver -d /tmp \
    && install -m755 /tmp/chromedriver-linux64/chromedriver /usr/local/bin/chromedriver \
    && rm -rf /tmp/cd.zip /tmp/chromedriver-linux64 \
    && chromedriver --version

WORKDIR /app
COPY --from=build /app/publish .

RUN mkdir -p wwwroot/ChromeProfiles wwwroot/Descargas wwwroot/2cap

COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

ENV DISPLAY=:99
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["/entrypoint.sh"]
