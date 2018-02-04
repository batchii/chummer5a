/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    /// <summary>
    /// Type of Contact.
    /// </summary>
    public enum ContactType
    {
        Contact = 0,
        Enemy = 1,
        Pet = 2,
    }

    /// <summary>
    /// A Contact or Enemy.
    /// </summary>
    public class Contact : INotifyPropertyChanged, IHasName, IHasMugshots
    {
        private string _strName = string.Empty;
        private string _strRole = string.Empty;
        private string _strLocation = string.Empty;
        private string _strUnique = Guid.NewGuid().ToString("D");

        private int _intConnection = 1;
        private int _intLoyalty = 1;
        private string _strMetatype = string.Empty;
        private string _strSex = string.Empty;
        private string _strAge = string.Empty;
        private string _strType = string.Empty;
        private string _strPreferredPayment = string.Empty;
        private string _strHobbiesVice = string.Empty;
        private string _strPersonalLife = string.Empty;

        private string _strGroupName = string.Empty;
        private ContactType _eContactType = ContactType.Contact;
        private string _strFileName = string.Empty;
        private string _strRelativeName = string.Empty;
        private Character _objLinkedCharacter;
        private string _strNotes = string.Empty;
        private Color _objColour;
        private bool _blnFree;
        private bool _blnIsGroup;
        private readonly Character _objCharacter;
        private bool _blnMadeMan;
        private bool _blnBlackmail;
        private bool _blnFamily;
        private bool _readonly;
        private bool _blnForceLoyalty;

        private readonly List<Image> _lstMugshots = new List<Image>();
        private int _intMainMugshotIndex = -1;

        public event PropertyChangedEventHandler PropertyChanged;

        #region Helper Methods
        /// <summary>
        /// Convert a string to a ContactType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static ContactType ConvertToContactType(string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
                return default(ContactType);
            switch (strValue)
            {
                case "Contact":
                    return ContactType.Contact;
                case "Pet":
                    return ContactType.Pet;
                default:
                    return ContactType.Enemy;
            }
        }
        #endregion

        #region Constructor, Save, Load, and Print Methods
        public Contact(Character objCharacter)
        {
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("contact");
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("role", _strRole);
            objWriter.WriteElementString("location", _strLocation);
            objWriter.WriteElementString("connection", _intConnection.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("loyalty", _intLoyalty.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("metatype", _strMetatype);
            objWriter.WriteElementString("sex", _strSex);
            objWriter.WriteElementString("age", _strAge);
            objWriter.WriteElementString("contacttype", _strType);
            objWriter.WriteElementString("preferredpayment", _strPreferredPayment);
            objWriter.WriteElementString("hobbiesvice", _strHobbiesVice);
            objWriter.WriteElementString("personallife", _strPersonalLife);
            objWriter.WriteElementString("type", _eContactType.ToString());
            objWriter.WriteElementString("file", _strFileName);
            objWriter.WriteElementString("relative", _strRelativeName);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("groupname", _strGroupName);
            objWriter.WriteElementString("colour", _objColour.ToArgb().ToString());
            objWriter.WriteElementString("free", _blnFree.ToString());
            objWriter.WriteElementString("group", _blnIsGroup.ToString());
            objWriter.WriteElementString("forceloyalty", _blnForceLoyalty.ToString());
            objWriter.WriteElementString("family", _blnFamily.ToString());
            objWriter.WriteElementString("blackmail", _blnBlackmail.ToString());

            if (ReadOnly) objWriter.WriteElementString("readonly", string.Empty);

            if (_strUnique != null)
            {
                objWriter.WriteElementString("guid", _strUnique);
            }

            SaveMugshots(objWriter);

            /* Disabled for now because we cannot change any properties in the linked character anyway
            if (LinkedCharacter?.IsSaving == false && !Program.MainForm.OpenCharacterForms.Any(x => x.CharacterObject == LinkedCharacter))
                LinkedCharacter.Save();
                */

            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Contact from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("role", ref _strRole);
            objNode.TryGetStringFieldQuickly("location", ref _strLocation);
            objNode.TryGetInt32FieldQuickly("connection", ref _intConnection);
            objNode.TryGetInt32FieldQuickly("loyalty", ref _intLoyalty);
            objNode.TryGetStringFieldQuickly("metatype", ref _strMetatype);
            objNode.TryGetStringFieldQuickly("sex", ref _strSex);
            objNode.TryGetStringFieldQuickly("age", ref _strAge);
            objNode.TryGetStringFieldQuickly("contacttype", ref _strType);
            objNode.TryGetStringFieldQuickly("preferredpayment", ref _strPreferredPayment);
            objNode.TryGetStringFieldQuickly("hobbiesvice", ref _strHobbiesVice);
            objNode.TryGetStringFieldQuickly("personallife", ref _strPersonalLife);
            _eContactType = ConvertToContactType(objNode["type"]?.InnerText);
            objNode.TryGetStringFieldQuickly("file", ref _strFileName);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            objNode.TryGetStringFieldQuickly("groupname", ref _strGroupName);
            objNode.TryGetBoolFieldQuickly("free", ref _blnFree);
            objNode.TryGetBoolFieldQuickly("group", ref _blnIsGroup);
            objNode.TryGetStringFieldQuickly("guid", ref _strUnique);
            objNode.TryGetBoolFieldQuickly("family", ref _blnFamily);
            objNode.TryGetBoolFieldQuickly("blackmail", ref _blnBlackmail);
            if (objNode["colour"] != null)
            {
                int intTmp = _objColour.ToArgb();
                if (objNode.TryGetInt32FieldQuickly("colour", ref intTmp))
                    _objColour = Color.FromArgb(intTmp);
            }

            if (objNode["readonly"] != null)
                _readonly = true;
            if (objNode["forceloyalty"] != null)
            {
                objNode.TryGetBoolFieldQuickly("forceloyalty", ref _blnForceLoyalty);
            }
            else if (objNode["mademan"] != null)
            {
                objNode.TryGetBoolFieldQuickly("mademan", ref _blnForceLoyalty);
            }

            RefreshLinkedCharacter(false);

            // Mugshots
            LoadMugshots(objNode);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("contact");
            objWriter.WriteElementString("name", Name);
            objWriter.WriteElementString("role", DisplayRoleMethod(strLanguageToPrint));
            objWriter.WriteElementString("location", Location);
            if (!IsGroup)
                objWriter.WriteElementString("connection", Connection.ToString(objCulture));
            else
                objWriter.WriteElementString("connection", LanguageManager.GetString("String_Group", strLanguageToPrint) + "(" + Connection.ToString(objCulture) + ')');
            objWriter.WriteElementString("loyalty", Loyalty.ToString(objCulture));
            objWriter.WriteElementString("metatype", DisplayMetatypeMethod(strLanguageToPrint));
            objWriter.WriteElementString("sex", DisplaySexMethod(strLanguageToPrint));
            objWriter.WriteElementString("age", DisplayAgeMethod(strLanguageToPrint));
            objWriter.WriteElementString("contacttype", DisplayTypeMethod(strLanguageToPrint));
            objWriter.WriteElementString("preferredpayment", DisplayPreferredPaymentMethod(strLanguageToPrint));
            objWriter.WriteElementString("hobbiesvice", DisplayHobbiesViceMethod(strLanguageToPrint));
            objWriter.WriteElementString("personallife", DisplayPersonalLifeMethod(strLanguageToPrint));
            objWriter.WriteElementString("type", LanguageManager.GetString("String_" + EntityType.ToString(), strLanguageToPrint));
            objWriter.WriteElementString("forceloyalty", ForceLoyalty.ToString());
            objWriter.WriteElementString("blackmail", Blackmail.ToString());
            objWriter.WriteElementString("family", Family.ToString());
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);

            PrintMugshots(objWriter);

            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties

        public bool ReadOnly
        {
            get => _readonly;
            set => _readonly = value;
        }


        /// <summary>
        /// Total points used for this contact.
        /// </summary>
        public int ContactPoints
        {
            get
            {
                if (Free) return 0;
                int intReturn = 0;
                if (Family) intReturn += 1;
                if (Blackmail) intReturn += 2;
                intReturn += Connection + Loyalty;
                return intReturn;
            }
        }

        /// <summary>
        /// Name of the Contact.
        /// </summary>
        public string Name
        {
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.CharacterName;
                return _strName;
            }
            set => _strName = value;
        }

        public string DisplayRoleMethod(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Role;

            return XmlManager.Load("contacts.xml", strLanguage).SelectSingleNode("/chummer/contacts/contact[text() = \"" + Role + "\"]/@translate")?.InnerText ?? Role;
        }

        public string DisplayRole
        {
            get
            {
                return DisplayRoleMethod(GlobalOptions.Language);
            }
            set
            {
                _strRole = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
            }
        }

        /// <summary>
        /// Role of the Contact.
        /// </summary>
        public string Role
        {
            get => _strRole;
            set => _strRole = value;
        }

        /// <summary>
        /// Location of the Contact.
        /// </summary>
        public string Location
        {
            get => _strLocation;
            set => _strLocation = value;
        }

        /// <summary>
        /// Contact's Connection Rating.
        /// </summary>
        public int Connection
        {
            get => _intConnection;
            set => _intConnection = value;
        }

        /// <summary>
        /// Contact's Loyalty Rating (or Enemy's Incidence Rating).
        /// </summary>
        public int Loyalty
        {
            get => _intLoyalty;
            set => _intLoyalty = value;
        }

        public string DisplayMetatypeMethod(string strLanguage)
        {
            string strReturn = Metatype;
            if (LinkedCharacter != null)
            {
                // Update character information fields.
                XmlDocument objMetatypeDoc = XmlManager.Load("metatypes.xml", strLanguage);
                XmlNode objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + LinkedCharacter.Metatype + "\"]");
                if (objMetatypeNode == null)
                {
                    objMetatypeDoc = XmlManager.Load("critters.xml", strLanguage);
                    objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + LinkedCharacter.Metatype + "\"]");
                }

                strReturn = objMetatypeNode["translate"]?.InnerText ?? LanguageManager.TranslateExtra(LinkedCharacter.Metatype, strLanguage);

                if (!string.IsNullOrEmpty(LinkedCharacter.Metavariant))
                {
                    objMetatypeNode = objMetatypeNode.SelectSingleNode("metavariants/metavariant[name = \"" + LinkedCharacter.Metavariant + "\"]");

                    string strMetatypeTranslate = objMetatypeNode["translate"]?.InnerText;
                    strReturn += !string.IsNullOrEmpty(strMetatypeTranslate) ? " (" + strMetatypeTranslate + ')' : " (" + LanguageManager.TranslateExtra(LinkedCharacter.Metavariant, strLanguage) + ')';
                }
            }
            else
                strReturn = LanguageManager.TranslateExtra(strReturn, strLanguage);
            return strReturn;
        }

        public string DisplayMetatype
        {
            get
            {
                return DisplayMetatypeMethod(GlobalOptions.Language);
            }
            set
            {
                _strMetatype = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
            }
        }

        /// <summary>
        /// Metatype of this Contact.
        /// </summary>
        public string Metatype
        {
            get
            {
                if (LinkedCharacter != null)
                {
                    string strMetatype = LinkedCharacter.Metatype;

                    if (!string.IsNullOrEmpty(LinkedCharacter.Metavariant))
                    {
                        strMetatype += " (" + LinkedCharacter.Metavariant + ')';
                    }
                    return strMetatype;
                }
                return _strMetatype;
            }
            set
            {
                _strMetatype = value;
            }
        }

        public string DisplaySexMethod(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Sex;

            return XmlManager.Load("contacts.xml", strLanguage).SelectSingleNode("/chummer/sexes/sex[text() = \"" + Sex + "\"]/@translate")?.InnerText ?? Sex;
        }

        public string DisplaySex
        {
            get
            {
                return DisplaySexMethod(GlobalOptions.Language);
            }
            set
            {
                _strSex = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
            }
        }

        /// <summary>
        /// Gender of this Contact.
        /// </summary>
        public string Sex
        {
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.Sex;
                return _strSex;
            }
            set
            {
                _strSex = value;
            }
        }

        public string DisplayAgeMethod(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Age;

            return XmlManager.Load("contacts.xml", strLanguage).SelectSingleNode("/chummer/ages/age[text() = \"" + Age + "\"]/@translate")?.InnerText ?? Age;
        }

        public string DisplayAge
        {
            get
            {
                return DisplayAgeMethod(GlobalOptions.Language);
            }
            set
            {
                _strAge = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
            }
        }

        /// <summary>
        /// How old is this Contact.
        /// </summary>
        public string Age
        {
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.Age;
                return _strAge;
            }
            set
            {
                _strAge = value;
            }
        }

        public string DisplayTypeMethod(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Type;

            return XmlManager.Load("contacts.xml", strLanguage).SelectSingleNode("/chummer/types/type[text() = \"" + Type + "\"]/@translate")?.InnerText ?? Type;
        }

        public string DisplayType
        {
            get
            {
                return DisplayTypeMethod(GlobalOptions.Language);
            }
            set
            {
                _strType = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
            }
        }

        /// <summary>
        /// What type of Contact is this.
        /// </summary>
        public string Type
        {
            get
            {
                return _strType;
            }
            set
            {
                _strType = value;
            }
        }

        public string DisplayPreferredPaymentMethod(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return PreferredPayment;

            return XmlManager.Load("contacts.xml", strLanguage).SelectSingleNode("/chummer/preferredpayments/preferredpayment[text() = \"" + PreferredPayment + "\"]/@translate")?.InnerText ?? PreferredPayment;
        }

        public string DisplayPreferredPayment
        {
            get
            {
                return DisplayPreferredPaymentMethod(GlobalOptions.Language);
            }
            set
            {
                _strPreferredPayment = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
            }
        }

        /// <summary>
        /// Preferred payment method of this Contact.
        /// </summary>
        public string PreferredPayment
        {
            get
            {
                return _strPreferredPayment;
            }
            set
            {
                _strPreferredPayment = value;
            }
        }

        public string DisplayHobbiesViceMethod(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return HobbiesVice;

            return XmlManager.Load("contacts.xml", strLanguage).SelectSingleNode("/chummer/hobbiesvices/hobbyvice[text() = \"" + HobbiesVice + "\"]/@translate")?.InnerText ?? HobbiesVice;
        }

        public string DisplayHobbiesVice
        {
            get
            {
                return DisplayHobbiesViceMethod(GlobalOptions.Language);
            }
            set
            {
                _strHobbiesVice = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
            }
        }

        /// <summary>
        /// Hobbies/Vice of this Contact.
        /// </summary>
        public string HobbiesVice
        {
            get
            {
                return _strHobbiesVice;
            }
            set
            {
                _strHobbiesVice = value;
            }
        }

        public string DisplayPersonalLifeMethod(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return PersonalLife;

            return XmlManager.Load("contacts.xml", strLanguage).SelectSingleNode("/chummer/personallives/personallife[text() = \"" + PersonalLife + "\"]/@translate")?.InnerText ?? PersonalLife;
        }

        public string DisplayPersonalLife
        {
            get
            {
                return DisplayPersonalLifeMethod(GlobalOptions.Language);
            }
            set
            {
                _strPersonalLife = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
            }
        }

        /// <summary>
        /// Personal Life of this Contact.
        /// </summary>
        public string PersonalLife
        {
            get
            {
                return _strPersonalLife;
            }
            set
            {
                _strPersonalLife = value;
            }
        }

        /// <summary>
        /// Is this contact a group contact?
        /// </summary>
        public bool IsGroup
        {
            get => _blnIsGroup;
            set
            {
                _blnIsGroup = value;

                if (value && !ForceLoyalty)
                {
                    _intLoyalty = 1;
                }
            }
        }

        public bool IsGroupOrMadeMan
        {
            get => IsGroup || MadeMan;
            set => IsGroup = value;
        }

        public bool LoyaltyEnabled => !IsGroup && !ForceLoyalty;

        public int ConnectionMaximum => !_objCharacter.Created ? (_objCharacter.FriendsInHighPlaces ? 12 : 6) : 12;

        public string QuickText => $"({Connection}/{(IsGroup ? $"{Loyalty}G" : Loyalty.ToString())})";

        /// <summary>
        /// The Contact's type, either Contact or Enemy.
        /// </summary>
        public ContactType EntityType
        {
            get => _eContactType;
            set => _eContactType = value;
        }

        public bool IsNotEnemy
        {
            get
            {
                return EntityType != ContactType.Enemy;
            }
        }

        /// <summary>
        /// Name of the save file for this Contact.
        /// </summary>
        public string FileName
        {
            get => _strFileName;
            set
            {
                if (_strFileName != value)
                {
                    _strFileName = value;
                    RefreshLinkedCharacter(!string.IsNullOrEmpty(value));
                }
            }
        }

        /// <summary>
        /// Relative path to the save file.
        /// </summary>
        public string RelativeFileName
        {
            get => _strRelativeName;
            set
            {
                if (_strRelativeName != value)
                {
                    _strRelativeName = value;
                    RefreshLinkedCharacter(!string.IsNullOrEmpty(value));
                }
            }
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }

        /// <summary>
        /// Group Name.
        /// </summary>
        public string GroupName
        {
            get => _strGroupName;
            set => _strGroupName = value;
        }

        /// <summary>
        /// Contact Colour.
        /// </summary>
        public Color Colour
        {
            get => _objColour;
            set => _objColour = value;
        }

        /// <summary>
        /// Whether or not this is a free contact.
        /// </summary>
        public bool Free
        {
            get => _blnFree;
            set => _blnFree = value;
        }
        /// <summary>
        /// Unique ID for this contact
        /// </summary>
        public string GUID
        {
            get
            {
                return _strUnique;
            }
        }

        /// <summary>
        /// Is this contact a made man
        /// </summary>
        public bool MadeMan
        {
            get => _blnMadeMan;
            set
            {
                _blnMadeMan = value;
                if (value)
                {
                    _intLoyalty = 3;
                }
            }
        }

        public bool NotMadeMan
        {
            get => !MadeMan;
        }

        public bool Blackmail
        {
            get => _blnBlackmail;
            set => _blnBlackmail = value;
        }

        public bool Family
        {
            get => _blnFamily;
            set => _blnFamily = value;
        }

        public bool ForceLoyalty
        {
            get => _blnForceLoyalty;
            set => _blnForceLoyalty = value;
        }

        public Character CharacterObject
        {
            get => _objCharacter;
        }

        public Character LinkedCharacter
        {
            get => _objLinkedCharacter;
        }

        public bool NoLinkedCharacter
        {
            get => _objLinkedCharacter == null;
        }

        public void RefreshForControl()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Loyalty)));
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Connection)));
            }
            RefreshLinkedCharacter(false);
        }

        public void RefreshLinkedCharacter(bool blnShowError)
        {
            Character _objOldLinkedCharacter = _objLinkedCharacter;
            _objCharacter.LinkedCharacters.Remove(_objLinkedCharacter);
            bool blnError = false;
            bool blnUseRelative = false;

            // Make sure the file still exists before attempting to load it.
            if (!File.Exists(FileName))
            {
                // If the file doesn't exist, use the relative path if one is available.
                if (string.IsNullOrEmpty(RelativeFileName))
                    blnError = true;
                else if (!File.Exists(Path.GetFullPath(RelativeFileName)))
                    blnError = true;
                else
                    blnUseRelative = true;

                if (blnError && blnShowError)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_FileNotFound", GlobalOptions.Language).Replace("{0}", FileName), LanguageManager.GetString("MessageTitle_FileNotFound", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if (!blnError)
            {
                string strFile = blnUseRelative ? Path.GetFullPath(RelativeFileName) : FileName;
                if (strFile.EndsWith(".chum5"))
                {
                    Character objOpenCharacter = Program.MainForm.OpenCharacters.FirstOrDefault(x => x.FileName == strFile);
                    if (objOpenCharacter != null)
                        _objLinkedCharacter = objOpenCharacter;
                    else
                        _objLinkedCharacter = Program.MainForm.LoadCharacter(strFile, string.Empty, false, false);
                    if (_objLinkedCharacter != null)
                        _objCharacter.LinkedCharacters.Add(_objLinkedCharacter);
                }
            }
            if (_objLinkedCharacter != _objOldLinkedCharacter)
            {
                if (_objOldLinkedCharacter != null)
                {
                    if (!Program.MainForm.OpenCharacters.Any(x => x.LinkedCharacters.Contains(_objOldLinkedCharacter) && x != _objOldLinkedCharacter))
                    {
                        Program.MainForm.OpenCharacters.Remove(_objOldLinkedCharacter);
                        _objOldLinkedCharacter.DeleteCharacter();
                        _objOldLinkedCharacter = null;
                    }
                }
                if (_objLinkedCharacter != null)
                {
                    if (string.IsNullOrEmpty(_strName) && Name != LanguageManager.GetString("String_UnnamedCharacter", GlobalOptions.Language))
                        _strName = Name;
                    if (string.IsNullOrEmpty(_strAge) && !string.IsNullOrEmpty(Age))
                        _strAge = Age;
                    if (string.IsNullOrEmpty(_strSex) && !string.IsNullOrEmpty(Sex))
                        _strSex = Sex;
                    if (string.IsNullOrEmpty(_strMetatype) && !string.IsNullOrEmpty(Metatype))
                        _strMetatype = Metatype;
                }
                PropertyChangedEventHandler objPropertyChanged = PropertyChanged;
                if (objPropertyChanged != null)
                {
                    objPropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                    objPropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Age)));
                    objPropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Sex)));
                    objPropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Metatype)));
                    objPropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(NoLinkedCharacter)));
                }
            }
        }
        #endregion

        #region IHasMugshots
        /// <summary>
		/// Character's portraits encoded using Base64.
		/// </summary>
		public IList<Image> Mugshots
        {
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.Mugshots;
                else
                    return _lstMugshots;
            }
        }

        /// <summary>
        /// Character's main portrait encoded using Base64.
        /// </summary>
        public Image MainMugshot
        {
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.MainMugshot;
                else if (MainMugshotIndex >= Mugshots.Count || MainMugshotIndex < 0)
                    return null;
                else
                    return Mugshots[MainMugshotIndex];
            }
            set
            {
                if (LinkedCharacter != null)
                    LinkedCharacter.MainMugshot = value;
                else
                {
                    if (value == null)
                    {
                        MainMugshotIndex = -1;
                        return;
                    }
                    int intNewMainMugshotIndex = Mugshots.IndexOf(value);
                    if (intNewMainMugshotIndex != -1)
                    {
                        MainMugshotIndex = intNewMainMugshotIndex;
                    }
                    else
                    {
                        Mugshots.Add(value);
                        MainMugshotIndex = Mugshots.Count - 1;
                    }
                }
            }
        }

        /// <summary>
        /// Index of Character's main portrait. -1 if set to none.
        /// </summary>
        public int MainMugshotIndex
        {
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.MainMugshotIndex;
                else
                    return _intMainMugshotIndex;
            }
            set
            {
                if (LinkedCharacter != null)
                    LinkedCharacter.MainMugshotIndex = value;
                else if (value >= _lstMugshots.Count || value < -1)
                    _intMainMugshotIndex = -1;
                else
                    _intMainMugshotIndex = value;
            }
        }

        public void SaveMugshots(XmlTextWriter objWriter)
        {
            objWriter.WriteElementString("mainmugshotindex", MainMugshotIndex.ToString());
            // <mugshot>
            objWriter.WriteStartElement("mugshots");
            foreach (Image imgMugshot in Mugshots)
            {
                objWriter.WriteElementString("mugshot", imgMugshot.ToBase64String());
            }
            // </mugshot>
            objWriter.WriteEndElement();
        }

        public void LoadMugshots(XmlNode xmlSavedNode)
        {
            xmlSavedNode.TryGetInt32FieldQuickly("mainmugshotindex", ref _intMainMugshotIndex);
            using (XmlNodeList xmlMugshotsList = xmlSavedNode.SelectNodes("mugshots/mugshot"))
            {
                if (xmlMugshotsList != null)
                {
                    List<string> lstMugshotsBase64 = new List<string>(xmlMugshotsList.Count);
                    foreach (XmlNode objXmlMugshot in xmlMugshotsList)
                    {
                        string strMugshot = objXmlMugshot.InnerText;
                        if (!string.IsNullOrWhiteSpace(strMugshot))
                        {
                            lstMugshotsBase64.Add(strMugshot);
                        }
                    }
                    if (lstMugshotsBase64.Count > 1)
                    {
                        Image[] objMugshotImages = new Image[lstMugshotsBase64.Count];
                        Parallel.For(0, lstMugshotsBase64.Count, i =>
                        {
                            objMugshotImages[i] = lstMugshotsBase64[i].ToImage(System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                        });
                        _lstMugshots.AddRange(objMugshotImages);
                    }
                    else if (lstMugshotsBase64.Count == 1)
                    {
                        _lstMugshots.Add(lstMugshotsBase64[0].ToImage(System.Drawing.Imaging.PixelFormat.Format32bppPArgb));
                    }
                }
            }
        }

        public void PrintMugshots(XmlTextWriter objWriter)
        {
            if (LinkedCharacter != null)
                LinkedCharacter.PrintMugshots(objWriter);
            else if (Mugshots.Count > 0)
            {
                // Since IE is retarded and can't handle base64 images before IE9, we need to dump the image to a temporary directory and re-write the information.
                // If you give it an extension of jpg, gif, or png, it expects the file to be in that format and won't render the image unless it was originally that type.
                // But if you give it the extension img, it will render whatever you give it (which doesn't make any damn sense, but that's IE for you).
                string strMugshotsDirectoryPath = Path.Combine(Application.StartupPath, "mugshots");
                if (!Directory.Exists(strMugshotsDirectoryPath))
                {
                    try
                    {
                        Directory.CreateDirectory(strMugshotsDirectoryPath);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_Insufficient_Permissions_Warning", GlobalOptions.Language));
                    }
                }
                Guid guiImage = Guid.NewGuid();
                string imgMugshotPath = Path.Combine(strMugshotsDirectoryPath, guiImage.ToString("N") + ".img");
                Image imgMainMugshot = MainMugshot;
                if (imgMainMugshot != null)
                {
                    imgMainMugshot.Save(imgMugshotPath);
                    // <mainmugshotpath />
                    objWriter.WriteElementString("mainmugshotpath", "file://" + imgMugshotPath.Replace(Path.DirectorySeparatorChar, '/'));
                    // <mainmugshotbase64 />
                    objWriter.WriteElementString("mainmugshotbase64", imgMainMugshot.ToBase64String());
                }
                // <othermugshots>
                objWriter.WriteElementString("hasothermugshots", (imgMainMugshot == null || Mugshots.Count > 1).ToString());
                objWriter.WriteStartElement("othermugshots");
                for (int i = 0; i < Mugshots.Count; ++i)
                {
                    if (i == MainMugshotIndex)
                        continue;
                    Image imgMugshot = Mugshots[i];
                    objWriter.WriteStartElement("mugshot");

                    objWriter.WriteElementString("stringbase64", imgMugshot.ToBase64String());

                    imgMugshotPath = Path.Combine(strMugshotsDirectoryPath, guiImage.ToString("N") + i.ToString() + ".img");
                    imgMugshot.Save(imgMugshotPath);
                    objWriter.WriteElementString("temppath", "file://" + imgMugshotPath.Replace(Path.DirectorySeparatorChar, '/'));

                    objWriter.WriteEndElement();
                }
                // </mugshots>
                objWriter.WriteEndElement();
            }
        }
        #endregion
    }
}