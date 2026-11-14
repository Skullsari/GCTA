local addonName = ...
local target = "target"

local function say(message)
    print("|cffffbf56GCTA|r " .. message)
end

local function rectangle(parent, width, height, r, g, b)
    local texture = parent:CreateTexture(nil, "OVERLAY")
    texture:SetSize(width, height)
    texture:SetColorTexture(r, g, b, 1)
    return texture
end

local card = CreateFrame("Frame", nil, UIParent)
card:SetFrameStrata("TOOLTIP")
card:SetSize(520, 184)
card:SetPoint("CENTER")
local background = card:CreateTexture(nil, "BACKGROUND")
background:SetAllPoints()
background:SetColorTexture(0.065, 0.075, 0.10, 1)
card:Hide()

-- These four blocks bound the only rectangle the companion can upload.
rectangle(card, 16, 16, 1, 0, 1):SetPoint("TOPLEFT")
rectangle(card, 16, 16, 0, 1, 1):SetPoint("TOPRIGHT")
rectangle(card, 16, 16, 1, 1, 0):SetPoint("BOTTOMLEFT")
rectangle(card, 16, 16, 0, 1, 0):SetPoint("BOTTOMRIGHT")

local panel = CreateFrame("Frame", nil, card)
panel:SetPoint("TOPLEFT", 24, -22)
panel:SetPoint("BOTTOMRIGHT", -24, 22)

local function panelLayer(inset, r, g, b)
    local texture = panel:CreateTexture(nil, "BACKGROUND", nil, inset)
    texture:SetPoint("TOPLEFT", inset, -inset)
    texture:SetPoint("BOTTOMRIGHT", -inset, inset)
    texture:SetColorTexture(r, g, b, 1)
end

-- Dark metal, a bronze edge and a recessed blue-black face.
panelLayer(0, 0.28, 0.22, 0.14)
panelLayer(1, 0.68, 0.53, 0.30)
panelLayer(2, 0.21, 0.18, 0.14)
panelLayer(5, 0.38, 0.31, 0.21)
panelLayer(6, 0.045, 0.064, 0.085)

for _, corner in ipairs({ "TOPLEFT", "TOPRIGHT", "BOTTOMLEFT", "BOTTOMRIGHT" }) do
    local x = corner:find("LEFT", 1, true) and 2 or -2
    local y = corner:find("TOP", 1, true) and -2 or 2
    rectangle(panel, 21, 3, 0.76, 0.61, 0.36):SetPoint(corner, panel, corner, x, y)
    rectangle(panel, 3, 16, 0.76, 0.61, 0.36):SetPoint(corner, panel, corner, x, y)
end

local portraitMount = panel:CreateTexture(nil, "BORDER")
portraitMount:SetSize(111, 111)
portraitMount:SetPoint("LEFT", 14, 0)
portraitMount:SetColorTexture(0.045, 0.064, 0.085, 1)

local portrait = panel:CreateTexture(nil, "ARTWORK")
portrait:SetSize(105, 105)
portrait:SetPoint("CENTER", portraitMount)
local portraitLabel = panel:CreateFontString(nil, "OVERLAY", "GameFontNormalHuge")
portraitLabel:SetPoint("CENTER", portrait)
portraitLabel:SetText("?")

local function label(template, y, align)
    local item = panel:CreateFontString(nil, "OVERLAY", template)
    item:SetPoint("TOPLEFT", 144, y)
    item:SetPoint("TOPRIGHT", -19, y)
    item:SetJustifyH(align or "LEFT")
    item:SetJustifyV("TOP")
    return item
end

local nameTop = -17
local nameLabel = label("GameFontNormalHuge", nameTop)
local nativeFont = nameLabel:GetFont()
local titleFont = "Fonts\\FRIZQT__.TTF"
local titleSize = 33
if not nameLabel:SetFont(titleFont, titleSize) then
    titleFont = nativeFont
end
nameLabel:SetTextColor(1, 0.86, 0.59)
nameLabel:SetMaxLines(2)

local detailsLabel = label("GameFontHighlight", -62)
detailsLabel:SetTextColor(0.91, 0.87, 0.79)
local guildLabel = label("GameFontHighlight", -55)
local guildFont, detailsSize, guildFlags = guildLabel:GetFont()
guildLabel:SetFont(guildFont, detailsSize + 3, guildFlags)
guildLabel:SetMaxLines(1)
guildLabel:SetTextColor(0.93, 0.78, 0.52)
local hunterLabel = label("GameFontHighlightSmall", -109, "RIGHT")
local hunterFont, hunterSize, hunterFlags = hunterLabel:GetFont()
hunterLabel:SetFont(hunterFont, hunterSize + 2, hunterFlags)
hunterLabel:SetTextColor(0.73, 0.70, 0.64)

local busy = false
local captureNumber = 0
local function finish()
    card:Hide()
    busy = false
end

local function readable(value)
    return (not issecretvalue or not issecretvalue(value))
        and type(value) == "string" and value ~= ""
end

local function characterName(unit)
    local firstName, surname = UnitFullName(unit)
    local name
    if GetUnitName then
        local ok, formatted = pcall(GetUnitName, unit)
        if ok then name = formatted end
    end
    -- On Forever the second result of UnitFullName is the surname.
    if readable(firstName) and readable(surname)
        and (not readable(name) or name == firstName) then
        return firstName .. " " .. surname
    end
    if readable(name) then return name end
    if readable(firstName) then return firstName end
end

local function capture()
    if busy then return end
    local exists = UnitExists(target)
    if issecretvalue and issecretvalue(exists) then
        say("Capture impossible : WoW masque l’identité de la cible.")
        return
    end
    if not exists then
        say("Cible un joueur.")
        return
    end
    local isPlayer = UnitIsPlayer(target)
    if issecretvalue and issecretvalue(isPlayer) then
        say("Capture impossible : WoW masque l’identité de la cible.")
        return
    end
    if not isPlayer then
        say("Ce n’est pas un joueur.")
        return
    end
    local name = characterName(target)
    if not name then
        say("Capture impossible : le nom de la cible est indisponible dans WoW.")
        return
    end
    local race = UnitRace(target)
    local class = UnitClass(target)
    nameLabel:SetFont(titleFont, titleSize)
    nameLabel:SetText(name)
    for size = titleSize - 1, 21, -1 do
        if nameLabel:GetUnboundedStringWidth() <= nameLabel:GetWidth() then break end
        nameLabel:SetFont(titleFont, size)
    end
    local guildOk, guild = pcall(GetGuildInfo, target)
    local showGuild = guildOk and readable(guild)
    guildLabel:SetShown(showGuild)
    local detailsTop = nameTop - nameLabel:GetStringHeight() - 5
    if showGuild then
        guildLabel:SetText((guild:gsub("|", "||")))
        guildLabel:SetPoint("TOPLEFT", 144, detailsTop)
        guildLabel:SetPoint("TOPRIGHT", -19, detailsTop)
        detailsTop = detailsTop - guildLabel:GetStringHeight() - 4
    end
    detailsLabel:SetPoint("TOPLEFT", 144, detailsTop)
    detailsLabel:SetPoint("TOPRIGHT", -19, detailsTop)
    detailsLabel:SetText((readable(class) and class or "Classe inconnue")
        .. "  ·  " .. (readable(race) and race or "Race inconnue"))
    hunterLabel:SetText("Capturé par " .. (characterName("player") or "un joueur inconnu"))

    portrait:SetTexture(nil)
    local hasPortrait = SetPortraitTexture and pcall(SetPortraitTexture, portrait, target, true)
    portraitLabel:SetShown(not hasPortrait or not portrait:GetTexture())
    busy = true
    captureNumber = captureNumber + 1
    local number = captureNumber
    GameTooltip:Hide()
    card:Show()
    C_Timer.After(0.20, function()
        if not busy or number ~= captureNumber then return end
        local ok = Screenshot and pcall(Screenshot)
        if not ok then
            say("Capture automatique indisponible. Appuie sur Impr. écran.")
        end
        C_Timer.After(ok and 2 or 6, function()
            if busy and number == captureNumber then
                finish()
                if ok then say("Vérifie le dossier Screenshots ou le compagnon.") end
            end
        end)
    end)
end

local button = CreateFrame("Button", "GCTACaptureButton", UIParent)
button:SetSize(44, 44)
button:SetPoint("CENTER", UIParent, "CENTER", 245, -125)
button:SetClampedToScreen(true)
button:SetMovable(true)
button:RegisterForDrag("LeftButton")
local orbTexture = "Interface\\AddOns\\GCTA\\Orb"
button:SetNormalTexture(orbTexture)
button:SetHighlightTexture(orbTexture, "ADD")
button:GetHighlightTexture():SetAlpha(0.25)
button:SetScript("OnClick", function()
    if not IsShiftKeyDown() then capture() end
end)
button:SetScript("OnDragStart", function(self)
    if IsShiftKeyDown() then self:StartMoving() end
end)
button:SetScript("OnDragStop", function(self)
    self:StopMovingOrSizing()
    local x, y = self:GetCenter()
    if x and y then
        GCTAPrefs.x = x - UIParent:GetWidth() / 2
        GCTAPrefs.y = y - UIParent:GetHeight() / 2
    end
end)
button:SetScript("OnEnter", function(self)
    if busy then return end
    GameTooltip:SetOwner(self, "ANCHOR_TOP")
    GameTooltip:AddLine("GCTA · Capturer la cible")
    GameTooltip:AddLine("Maj + glisser pour déplacer", 1, 1, 1)
    GameTooltip:Show()
end)
button:SetScript("OnLeave", function() GameTooltip:Hide() end)

local events = CreateFrame("Frame")
events:RegisterEvent("ADDON_LOADED")
events:RegisterEvent("SCREENSHOT_SUCCEEDED")
events:RegisterEvent("SCREENSHOT_FAILED")
events:SetScript("OnEvent", function(_, event, name)
    if event == "ADDON_LOADED" then
        if name ~= addonName then return end
        GCTAPrefs = type(GCTAPrefs) == "table" and GCTAPrefs or {}
        if type(GCTAPrefs.x) == "number" and type(GCTAPrefs.y) == "number" then
            button:ClearAllPoints()
            button:SetPoint("CENTER", UIParent, "CENTER", GCTAPrefs.x, GCTAPrefs.y)
        end
    elseif busy then
        finish()
        say(event == "SCREENSHOT_SUCCEEDED" and "Capture enregistrée sur le PC."
            or "WoW n’a pas pu enregistrer la capture.")
    end
end)
