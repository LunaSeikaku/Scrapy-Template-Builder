using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;

namespace XPath_Template
{
    public partial class Form1 : Form
    {
        IWebDriver driver;
        IJavaScriptExecutor jsExecutor;
        IWebElement ele;

        public Form1()
        {
            InitializeComponent();
            cb_assigned_to.SelectedIndex = 0;
            cb_status.SelectedIndex = 0;
        }

        private void cb_infinite_scroll_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_infinite_scroll.Checked) { lb_next_page.Text = "Next Page URL:"; }
            else { lb_next_page.Text = "Next Page XPath:"; }
        }
        // returns true if 1-or-more input fields are empty, otherwise returns false
        private bool no_fields_are_empty()
        {
            if (tb_domain.TextLength < 3 |
                rtb_urls.TextLength < 3 | //tb_url.TextLength < 3 |
                tb_boat_listings.TextLength < 3 |
                tb_next_page.TextLength < 3 |
                tb_boat_make.TextLength < 3 |
                tb_boat_model.TextLength < 3 |
                tb_boat_year.TextLength < 3 |
                tb_boat_condition.TextLength < 3 |
                tb_boat_price.TextLength < 3 |
                tb_boat_length.TextLength < 3 |
                tb_boat_material.TextLength < 3 |
                tb_boat_location.TextLength < 3 |
                tb_boat_country.TextLength < 3 |
                cb_assigned_to.Text.Length < 3 |
                cb_status.Text.Length < 3)
                //rtb_notes.Text.Trim().Length < 3)
            { return false; }
            return true;
        }
        private void tb_spider_denomer_TextChanged(object sender, EventArgs e)
        {
            // check if we can Create Template! with current input:
            tb_spider_denomer.Text = tb_spider_denomer.Text.Trim();
            confirm_we_can_create_template();

            // only proceed if on Load Template...
            if (btn_load_template.Text == "Clear Template!") { return; }

            // grab root scrapy project directory
            string folder_path = Directory.GetCurrentDirectory();

            // grab filepath from below the source directory using the spider name:
            string file_path = $@"{folder_path}\yachtscrape\spiders\{tb_spider_denomer.Text.ToLower()}.py";

            // if file exists, enable Load Template button - otherwise disable it
            if (File.Exists(file_path)) { btn_load_template.Enabled = true; }
            else { btn_load_template.Enabled = false; }

            //if (tb_spider_denomer.TextLength < 3 & btn_load_template.Text=="Load Template...") {  }
            //else {  }
        }
        private void tb_domain_TextChanged(object sender, EventArgs e)
        {
            // check if we can Create Template! with current input:
            tb_domain.Text = tb_domain.Text.Trim().ToLower();
            confirm_we_can_create_template();

            // only proceed if on Load Template...
            if (btn_load_template.Text == "Clear Template!") { return; }

            // get filename which is also the spider class name and spider name:
            string spider_name = tb_domain.Text.Split('.')[0].ToLower();

            // grab root scrapy project directory
            string folder_path = Directory.GetCurrentDirectory();

            // grab filepath from below the source directory using the spider name:
            string file_path = $@"{folder_path}\yachtscrape\spiders\{spider_name}.py";

            // if file exists, enable Load Template button - otherwise disable it
            if (File.Exists(file_path)) { btn_load_template.Enabled = true; }
            else { btn_load_template.Enabled = false; }
        }
        private void rtb_urls_TextChanged(object sender, EventArgs e)
        {
            confirm_we_can_create_template();//trimming of rtb_urls done in the RegEx function of create_template()

            // get every base url into a single one-line string (and validate it at the same time for efficency):
            Regex rx_url = new Regex(
@"^http(s)?:\/\/(www\.){0,1}((?!-)[A-Za-z0-9-]{1,63}(?<!-)){1}(\.[A-Za-z]{2,6}){1,2}(.)*$");//'{0,1}'='?' but no C# handling for multiple '?' LOL
            foreach (var s in rtb_urls.Lines)
            {
                string strim = s.Trim();//.ToLower();//urls can be case-sensitive apparently (TMYK)
                if (!rx_url.IsMatch(strim)) { btn_selenium.Enabled = false; return; }
            }
            if (rtb_urls.Lines.Length > 0)
            {
                btn_selenium.Enabled = true;//if the compiler gets to this line, every line is a valid url so it's okay!
            }
        }
        private void border_control(string xpath)
        {
            if (xpath=="None" | jsExecutor==null) { return; }

            // first remove any existing inline CSS:
            if (ele != null) 
            {
                try { jsExecutor?.ExecuteScript("arguments[0].style.border='0px'", ele); } catch (WebDriverException) { return; }
                jsExecutor?.ExecuteScript("arguments[0].style.backgroundColor='transparent'", ele);
            }

            // then attempt to make an element from the XPath input:
            try 
            { 
                ele = driver?.FindElement(By.XPath(xpath)); 
            }
            catch (Exception e) 
            { 
                if (e is WebDriverException) { driver = null; jsExecutor = null; }//declare_selenium(); driver.Close();
                return; 
            }

            // if the previous line was successful, make a border around the new item and scroll to it:
            jsExecutor = (IJavaScriptExecutor)driver;
            jsExecutor.ExecuteScript("arguments[0].style.border='5px solid black'", ele);
            jsExecutor.ExecuteScript("arguments[0].style.backgroundColor='#22AAE2'", ele);
            jsExecutor.ExecuteScript("arguments[0].scrollIntoView(true);", ele);
        }
        private void tb_boat_listings_TextChanged(object sender, EventArgs e)
        {
            // check if we can Create Template! with current input:
            tb_boat_listings.Text = tb_boat_listings.Text.Replace("'", "\"").Trim();
            confirm_we_can_create_template();

            border_control(tb_boat_listings.Text);
        }
        private void tb_next_page_TextChanged(object sender, EventArgs e)
        {
            // check if we can Create Template! with current input:
            tb_next_page.Text = tb_next_page.Text.Replace("'", "\"").Trim();
            confirm_we_can_create_template();

            border_control(tb_next_page.Text);
        }
        private void tb_specifications_TextChanged(object sender, EventArgs e)
        {
            // check if we can Create Template! with current input:
            tb_specifications.Text = tb_specifications.Text.Replace("'", "\"").Trim();
            //confirm_we_can_create_template();
            string specs = "";
            if (tb_specifications.Text.Length > 0)
            {
                specs = tb_specifications.Text.Replace("(", "");
                if (specs.Length > 0) { specs = specs.Substring(0, 1); }
            }
            if (specs != "/")//& specs != "."
            {
                tb_specifications.Text = "None";
            }

            border_control(tb_specifications.Text);
        }
        private void boat_listing_xpath_validation(TextBox xpath_textbox, NumericUpDown start, NumericUpDown stop, CheckBox sold_or_not)
        {
            // check if we can Create Template! with current input (make or model check):
            tooltip.SetToolTip(xpath_textbox, "");
            xpath_textbox.Text = xpath_textbox.Text.Replace("'", "\"").Trim();
            confirm_we_can_create_template();

            // validate input:
            string first_char = ""; // has to be string as it might not always be the first character
            if (xpath_textbox.Text.Length > 0)
            {
                first_char = xpath_textbox.Text.Replace("(", "");
                if (first_char.Length > 0) { first_char = first_char.Substring(0, 1); }
            }
            if (first_char == "/" | (first_char == "." & tb_specifications.Text != "None"))
            {
                start.Enabled = true;
                stop.Enabled = true;
                sold_or_not.Enabled = true;

                if (first_char == "." & tb_specifications.Text != "None")
                {
                    string absolute_xpath = tb_specifications.Text + xpath_textbox.Text.Substring(1);
                    tooltip.SetToolTip(xpath_textbox, absolute_xpath);
                    border_control(absolute_xpath.Replace("/@href","").Replace("/text()", ""));
                }
                else { border_control(xpath_textbox.Text); } 
            }
            else
            {
                xpath_textbox.Text = "None";
                start.Enabled = false;
                stop.Enabled = false;
                sold_or_not.Enabled = false;
                sold_or_not.Checked = false;
                start.Value = 0;
                stop.Value = 0;
                if (first_char == "." & tb_specifications.Text == "None") { error_box("To be able to grab data from a boat listing's specifications, you first need to define the Specifications XPath!"); }
            }
        }
        private void tb_boat_make_TextChanged(object sender, EventArgs e)
        {
            boat_listing_xpath_validation(tb_boat_make, ud_make_a, ud_make_b, cb_sold_make);
        }
        private void tb_boat_model_TextChanged(object sender, EventArgs e)
        {
            boat_listing_xpath_validation(tb_boat_model, ud_model_a, ud_model_b, cb_sold_model);
        }
        private void tb_boat_year_TextChanged(object sender, EventArgs e)
        {
            boat_listing_xpath_validation(tb_boat_year, ud_year_a, ud_year_b, cb_sold_year);
        }
        private void tb_boat_condition_TextChanged(object sender, EventArgs e)
        {
            boat_listing_xpath_validation(tb_boat_condition, ud_condition_a, ud_condition_b, cb_sold_condition);
        }
        private void tb_boat_price_TextChanged(object sender, EventArgs e)
        {
            boat_listing_xpath_validation(tb_boat_price, ud_price_a, ud_price_b, cb_sold_price);
        }
        private void tb_boat_length_TextChanged(object sender, EventArgs e)
        {
            boat_listing_xpath_validation(tb_boat_length, ud_length_a, ud_length_b, cb_sold_length);
        }
        private void tb_boat_material_TextChanged(object sender, EventArgs e)
        {
            boat_listing_xpath_validation(tb_boat_material, ud_material_a, ud_material_b, cb_sold_material);
        }
        private void tb_boat_location_TextChanged(object sender, EventArgs e)
        {
            boat_listing_xpath_validation(tb_boat_location, ud_location_a, ud_location_b, cb_sold_location);
        }
        private void tb_boat_country_TextChanged(object sender, EventArgs e)
        {
            boat_listing_xpath_validation(tb_boat_country, ud_country_a, ud_country_b, cb_sold_country);
        }

        private void btn_Create_Click(object sender, EventArgs e)
        {
            create_template(true);
        }
        private void btn_debug_Click(object sender, EventArgs e)
        {
            create_template(false,"pass#");// overwrite template to temporary put it into Debug Mode
            crawl_spider('c');// run scrapy on temporary template (no pagination)
            create_template();// reset template back to original
        }
        private void btn_test_Click(object sender, EventArgs e)
        {
            create_template();// ensure template is compiled before running
            crawl_spider('c');
        }

        private void cb_parse_gbp_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_parse_gbp.Checked) { cb_startchar.Enabled = true; }
            else { cb_startchar.Enabled = false; }
        }
        private void cb_parse_feet_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_parse_feet.Checked) { cb_convert_metres_to_feet.Enabled = true; }
            else { cb_convert_metres_to_feet.Enabled = false; }
        }
        private void btn_csv_Click(object sender, EventArgs e)
        {
            crawl_spider('n');
        }
        private void btn_load_template_Click(object sender, EventArgs e)
        {
            if (btn_load_template.Text == "Clear Template!") { clear_template(); }
            else { load_template(); }
        }

        // Custom Functions:
        private void error_box(string msg)
        {
            MessageBox.Show(
            msg,
            "Error!",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
            );
        }
        private void success_box(string msg)
        {
            MessageBox.Show(
            msg,
            "Information!",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
            );
        }
        private string format_yield(string specs_xpath, decimal a, decimal b, string sub = "Unknown")
        {
            // first we confirm if there is an xpath to parse or not:
            if (specs_xpath=="None") { return $"'{sub}',\n"; }// no xpath to parse :)

            // if there IS an xpath to parse, we first confirm where the specs_xpath is coming from:
            string specs_source = "specs_table";// specs_xpath is from specifications table xpath
            if (specs_xpath.Replace("(", "").Substring(0, 2) == "//") { specs_source = "response"; }// specs_xpath is from base HTML

            // determine if we need any substring list concatenation nonsense and add it if so:
            string output = "";
            if (a != 0 | b != 0)// if either is true then we substring where xpath is not default value:
            {
                output = $"{specs_source}.xpath('{specs_xpath}').get().strip()[{a}:{b}] if {specs_source}.xpath('{specs_xpath}').get(default='{sub}')!='{sub}'else'{sub}',\n".Replace("[0:", "[:").Replace(":0]", ":]");
            }
            else// otherwise we just return a standard xpath:
            {
                output = $"{specs_source}.xpath('{specs_xpath}').get(default='{sub}').strip(),\n";
            }

            // return fully-formed output:
            return output;
        }
        private void confirm_we_can_create_template()
        {
            if (no_fields_are_empty())
            { 
                btn_Create.Enabled = true;
                btn_debug.Enabled = true;
                btn_test.Enabled = true;
            } else {
                btn_Create.Enabled = false;
                btn_debug.Enabled = false;
                btn_test.Enabled = false;
            }
        }
        private void create_template(bool show_box=false,string debugmode="")//debugmode="pass#" if Debug Mode is ON
        {
            if (no_fields_are_empty())//all form fields filled with something:
            {
                // get filename which is also the spider class name and spider name:
                string spider_name = tb_domain.Text.Split('.')[0].ToLower();

                // input validation:
                Regex rx_nam = new Regex(//confirm there is a valid spidername (no symbols basically)
@"^[A-Za-z0-9]{1,63}$");
                //string nam = tb_spider_denomer.Text;
                if (!rx_nam.IsMatch(spider_name)) { error_box("That is not a valid Spider Name!\n\n(at least 1 character long with no symbols, please!)"); return; }

                Regex rx_dom = new Regex(//confirm there is a valid "domain-name+.+TLD"
@"^((?!-)[A-Za-z0-9-]{1,63}(?<!-)){1}(\.[A-Za-z]{2,6}){1,2}$");//yes I know {1} is a meaningless quantifier: it's there as a reminder!
                string dom = tb_domain.Text;
                if (!rx_dom.IsMatch(dom)) { error_box("That is not a valid Website Domain!"); return; }

                // get every base url into a single one-line string (and validate it at the same time for efficency):
                Regex rx_url = new Regex(
@"^http(s)?:\/\/(www\.){0,1}((?!-)[A-Za-z0-9-]{1,63}(?<!-)){1}(\.[A-Za-z]{2,6}){1,2}(.)*$");//'{0,1}'='?' but no C# handling for multiple '?' LOL
                string urls = "";
                foreach (var s in rtb_urls.Lines)
                {
                    string strim = s.Trim();//.ToLower();//urls can be case-sensitive apparently (TMYK)
                    if (!rx_url.IsMatch(strim)) { error_box("One of the Website URLs is not a valid URL!"); return; }
                    urls = $"f'{strim}'," + urls;
                }
                urls = urls.Remove(urls.Length - 1);// remove trailing , symbol

                Regex rx_xpath = new Regex(//confirm all xpaths are indeed xpaths:
@"^\(*(\/|\.)\/(.)+$");
                string x = tb_boat_listings.Text;
                if (!rx_xpath.IsMatch(x)) { error_box("That is not a valid Boat Listing XPath!"); return; }
                x = tb_next_page.Text;
                if (!rx_xpath.IsMatch(x) & !cb_infinite_scroll.Checked) { error_box("That is not a valid Next Page XPath!"); return; }
                else if (!rx_url.IsMatch(x) & cb_infinite_scroll.Checked) { error_box("That is not a valid Infinite Scroll URL!"); return; }
                x = tb_specifications.Text;
                if (!rx_xpath.IsMatch(x) & x != "None") { error_box("That is not a valid Specifications XPath!"); return; }
                x = tb_boat_make.Text;
                if (!rx_xpath.IsMatch(x) & x != "None") { error_box("That is not a valid Boat Make XPath!"); return; }
                x = tb_boat_model.Text;
                if (!rx_xpath.IsMatch(x) & x != "None") { error_box("That is not a valid Boat Model XPath!"); return; }
                x = tb_boat_year.Text;
                if (!rx_xpath.IsMatch(x) & x != "None") { error_box("That is not a valid Boat Year XPath!"); return; }
                x = tb_boat_condition.Text;
                if (!rx_xpath.IsMatch(x) & x != "None") { error_box("That is not a valid Boat Condition XPath!"); return; }
                x = tb_boat_price.Text;
                if (!rx_xpath.IsMatch(x) & x != "None") { error_box("That is not a valid Boat Price XPath!"); return; }
                x = tb_boat_length.Text;
                if (!rx_xpath.IsMatch(x) & x != "None") { error_box("That is not a valid Boat Length XPath!"); return; }
                x = tb_boat_material.Text;
                if (!rx_xpath.IsMatch(x) & x != "None") { error_box("That is not a valid Boat Material XPath!"); return; }
                x = tb_boat_location.Text;
                if (!rx_xpath.IsMatch(x) & x != "None") { error_box("That is not a valid Boat Location XPath!"); return; }
                x = tb_boat_country.Text;
                if (!rx_xpath.IsMatch(x) & x != "None") { error_box("That is not a valid Boat Country XPath!"); return; }

                // end of input validation!

                // add suffixes where applicable: (CANT DO THIS IN CASE THEY NEED SOMETHING OTHER THAN text() END OF XPATH)
                //string make_suffix = "";
                //if (tb_boat_make.Text.Substring(tb_boat_make.TextLength-6)!="text()") { }

                // metadata:
                string assigned_to = cb_assigned_to.Text;
                string brief_status = cb_status.Text;
                string notes = rtb_notes.Text;
                if (notes != "") { notes = "\n" + notes; }

                // determine what fields can have the word "sold" in them (for removal purposes):
                string sold_make = "";
                if (cb_sold_make.Checked) { sold_make = "\t\tif string_contains_word(specifications['make']):yield None\n"; }
                string sold_model = "";
                if (cb_sold_model.Checked) { sold_model = "\t\tif string_contains_word(specifications['model']):yield None\n"; }
                string sold_year = "";
                if (cb_sold_year.Checked) { sold_year = "\t\tif string_contains_word(specifications['year']):yield None\n"; }
                string sold_condition = "";
                if (cb_sold_condition.Checked) { sold_condition = "\t\tif string_contains_word(specifications['condition']):yield None\n"; }
                string sold_price = "";
                if (cb_sold_price.Checked) { sold_price = "\t\tif string_contains_word(specifications['price']):yield None\n"; }
                string sold_material = "";
                if (cb_sold_material.Checked) { sold_material = "\t\tif string_contains_word(specifications['hull_material']):yield None\n"; }
                string sold_location = "";
                if (cb_sold_location.Checked) { sold_location = "\t\tif string_contains_word(specifications['location']):yield None\n"; }
                string sold_country = "";
                if (cb_sold_country.Checked) { sold_country = "\t\tif string_contains_word(specifications['country']):yield None\n"; }
                string sold = sold_make + sold_model + sold_year + sold_condition + sold_price + sold_material + sold_location + sold_country;
                if (sold!="") { sold = "\n\t\t# Remove sold boats:\n" + sold + "\n"; }

                // infinite scroll stuff:
                string infinite_scroll_headers = "";
                if (cb_infinite_scroll.Checked) { infinite_scroll_headers = "\tpage_number=1\n"; } // only 1 header for now
                string next_page_link_a = $"\t\tnext_page = response.xpath('{tb_next_page.Text}').get(default=[])\n";
                string next_page_link_b = "\t\tif next_page:# if a next page button exists, click it!\n";
                string next_page_link_c = $"\t\t\t{debugmode}yield scrapy.Request(url=response.urljoin(next_page), callback=self.parse)\n";
                if (cb_infinite_scroll.Checked)
                { 
                    next_page_link_a = "\t\tif boat_listing != None:#if not the end of the infinite scroll:\n";
                    next_page_link_b = "\t\t\tself.page_number+=1\n";
                    next_page_link_c = $"\t\t\t{debugmode}yield scrapy.Request(url=f'{tb_next_page.Text}', callback=self.parse)\n";
                }
                // form yield output:
                string make = format_yield(tb_boat_make.Text, ud_make_a.Value, ud_make_b.Value);
                string model = format_yield(tb_boat_model.Text, ud_model_a.Value, ud_model_b.Value);
                string year = format_yield(tb_boat_year.Text, ud_year_a.Value, ud_year_b.Value, "0");
                string condition = format_yield(tb_boat_condition.Text, ud_condition_a.Value, ud_condition_b.Value);
                string price = format_yield(tb_boat_price.Text, ud_price_a.Value, ud_price_b.Value, "0");
                string length = format_yield(tb_boat_length.Text, ud_length_a.Value, ud_length_b.Value, "0");
                string material = format_yield(tb_boat_material.Text, ud_material_a.Value, ud_material_b.Value);
                string location = format_yield(tb_boat_location.Text, ud_location_a.Value, ud_location_b.Value);
                string country = format_yield(tb_boat_country.Text, ud_country_a.Value, ud_country_b.Value);
                // specs xpath toggle:
                string specs_table = "";
                if (tb_specifications.Text!="None") { specs_table = $"\t\tspecs_table = response.xpath('{tb_specifications.Text}')\n"; }

                // post-processing handling:
                string absolute_url = "";
                if (cb_absolute_url.Checked) { absolute_url = "\t\t\thref = response.urljoin(href)# combine domain with href to form a full absolute_url\n"; }
                string parse_feet = "";
                if (cb_parse_feet.Checked == true) 
                {
                    char foot_mode = 'f';
                    if (cb_convert_metres_to_feet.Checked) { foot_mode = 'm'; }
                    parse_feet = $"\t\tspecifications['length'] = parse_feet(specifications['length'],'{foot_mode}')\n";
                }
                //string convert_metres_to_feet = "";
                //if (cb_convert_metres_to_feet.Checked) { convert_metres_to_feet = "\t\tspecifications['length'] = convert_metres_to_feet(specifications['length'])\n"; }
                string parse_gbp = "";
                if (cb_parse_gbp.Checked == true) { parse_gbp = $"\t\tspecifications['price'] = parse_gbp(specifications['price'],'{cb_startchar.Text}')\n"; }
                string post_processing = "";
                if (parse_feet != "" | parse_gbp != "" | sold != "") { post_processing = "\t\t# Post-Processing Begins:\n"; }

                string file_content = "import scrapy\n" +
"from ._useful_functions import *\n" +
"\n\"\"\"\n" +
$"Assigned To: {assigned_to}\nBrief Status: {brief_status}{notes}"+//metadata
"\n\"\"\"\n\n" +
$"class {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(spider_name)}Spider(scrapy.Spider):\n" +
$"\tname = '{spider_name}'\n" +
$"\tallowed_domains = ['{tb_domain.Text}']\n" +
$"{infinite_scroll_headers}" +//only appear if required
$"\tstart_urls = [{urls}]\n" +
"\n" +
"\tdef parse(self, response):\n" +
"\n" +
"\t\t# Grab details on every Boat Listing from this web page's response:\n" +
$"\t\tfor boat_listing in response.xpath('{tb_boat_listings.Text}'):\n" +
"\n" +
"\t\t\t# grab the boat_listing's full URL:\n" +
"\t\t\thref = boat_listing.get()# get just the href without the array stuff\n" +
$"{absolute_url}" +// appends href to domain url if the href is not a full url like "/boat-123"
"\n" +
"\t\t\t# run parse_boat(href) and return the boat listing:\n" +
"\t\t\tspecifications = response.follow(url=href, callback=self.parse_boat,meta={'URL':href})\n" +
"\t\t\tif not specifications:\n" +
"\t\t\t\tcontinue# if specifications returns as False, continue to next boat because that means there is a need to remove it from results!\n" +
"\t\t\tyield specifications# otherwise, return it as is!\n" +
"\n" +
"\t\t# Pagination Begins:\n" +
$"{next_page_link_a}{next_page_link_b}{next_page_link_c}"+
"\n" +
"\tdef parse_boat(self,response):\n" +
"\n" +
"\t\t# Define the full-formed url of the boat listing as a value from the parse() that called this function:\n" +
"\t\thref = response.request.meta['URL']\n" +
"\n" +
"\t\t# Define and get what specifications we desire:\n" +
$"{specs_table}" +
"\t\tspecifications = {\n" +
$"\t\t\t'make':{make}" +
$"\t\t\t'model':{model}" +
$"\t\t\t'year':{year}" +
$"\t\t\t'condition':{condition}" +
$"\t\t\t'price':{price}" +
$"\t\t\t'length':{length}" +
$"\t\t\t'hull_material':{material}" +
$"\t\t\t'location':{location}" +
$"\t\t\t'country':{country}" +
"\t\t\t'url':href.split('?')[0]}# remove any trailing query to save on memory space and prevent duplicates\n" +
"\n" +
$"{post_processing}{sold}{parse_feet}{parse_gbp}" +//convert_metres_to_feet
"\n" +
"\t\tyield specifications";

                // grab root scrapy project directory
                string folder_path = Directory.GetCurrentDirectory();

                string file_path = $@"{folder_path}\yachtscrape\spiders\{spider_name}.py";

                try { File.WriteAllText(file_path, file_content); }
                catch (IOException) { error_box($"Please close {file_path} before running this!"); return; }

                btn_load_template.Text = "Clear Template!";
                btn_load_template.Enabled = true;

                if (show_box) { success_box($"{spider_name}.py template file was created successfully!"); }
            }
            else//at least one field has not been filled in:
            {
                error_box("Please ensure every field has an input before submitting!");
            }
        }
        private void load_template()
        {
            // get filename which is also the spider class name and spider name:
            string spider_name = tb_domain.Text.Split('.')[0];

            // grab root scrapy project directory
            string folder_path = Directory.GetCurrentDirectory();

            // grab filepath from below the source directory using the spider name:
            string file_path = $@"{folder_path}\yachtscrape\spiders\{spider_name.ToLower()}.py";
            if (File.Exists(file_path) == false)
            { error_box($"Could not find {spider_name}.py!\n\nPlease enter an existing Spider Name!"); return; }

            // get every line from above file in an array:
            string[] lines = new string[] { };
            try { lines = File.ReadAllLines(file_path); }
            catch { error_box($"Could not read {spider_name}.py?"); return; }
            // If data found, Spider Name is legit so it remains unchanged

            // blank values first (false to exclude spider denomer):
            clear_template(false);

            // read remaining desired data from lines:
            int next = -2;
            int find, start, finish;
            string[] split;
            foreach (string l in lines)
            {
                switch (next)
                {
                    case -2:
                        find = l.IndexOf("Assigned");
                        if (find > -1) { find = l.IndexOf(": ") + 2; cb_assigned_to.SelectedItem = l.Substring(find); next += 1; }
                        continue;
                    case -1:
                        find = l.IndexOf("Brief");
                        if (find > -1) { find = l.IndexOf(": ") + 2; cb_status.SelectedItem = l.Substring(find); next += 1; }
                        continue;
                    case 0:
                        find = l.IndexOf("\"\"\"");
                        if (find > -1) { if (rtb_notes.Text.Length > 0) { rtb_notes.Text = rtb_notes.Text.Substring(1); } next += 1; }
                        else { rtb_notes.Text += "\n" + l; }
                        continue;
                    case 1:
                        find = l.IndexOf("allowed_domains");
                        if (find > -1) { find = l.IndexOf('[') + 2; tb_domain.Text = l.Substring(find, l.IndexOf(']') - find - 1); next += 1; }
                        continue;
                    case 2:
                        find = l.IndexOf("start_urls");
                        if (find > -1)
                        {
                            find = l.IndexOf('[') + 3;
                            string[] urls = l.Substring(find, l.IndexOf("']") - find).Split(new[] { "',f'" }, StringSplitOptions.None);//tb_url.Text
                            rtb_urls.Text = string.Join(Environment.NewLine, urls);
                            next += 1;
                        }
                        continue;
                    case 3:
                        find = l.IndexOf("for boat_listing");
                        if (find > -1) { find = l.IndexOf('(') + 2; tb_boat_listings.Text = l.Substring(find, l.IndexOf("'):") - find); next += 1; }
                        continue;
                    case 4:
                        find = l.IndexOf("urljoin");
                        if (find > -1) { cb_absolute_url.Checked = true; next += 1; }
                        else { if (l.IndexOf("follow") > -1) { next += 1; } }// no urljoin :(
                        continue;
                    case 5:
                        find = l.IndexOf("next_page");
                        if (find > -1) { find = l.IndexOf('(') + 2; tb_next_page.Text = l.Substring(find, l.IndexOf("').") - find); next += 1; }
                        else//infinite scroll stuff:
                        {
                            find = l.IndexOf("url=f'");
                            if (find > -1)
                            {
                                tb_next_page.Text = l.Substring(find + 6, l.IndexOf("', c") - find - 6);
                                cb_infinite_scroll.Checked = true;
                                next += 1;
                            }
                        }
                        continue;
                    case 6:
                        find = l.IndexOf("specs_table =");
                        if (find > -1) { find = l.IndexOf('(') + 2; tb_specifications.Text = l.Substring(find, l.IndexOf("')") - find); next += 1; }
                        else if (l.IndexOf("specifications = {") > -1) { next += 1; }
                        continue;
                    case 7:
                        find = l.IndexOf("make");
                        if (find > -1)
                        {
                            if (l.IndexOf("Unknown',") > -1) { tb_boat_make.Text = "None"; next += 1; continue; }
                            find = l.IndexOf("('") + 2;
                            tb_boat_make.Text = l.Substring(find, l.IndexOf("').") - find);
                            start = l.IndexOf("()[") + 3;
                            finish = l.IndexOf("] ");//tail = tail.Remove(tail.Length - 2);
                            if (start > 1 & finish > -1)
                            {
                                split = l.Substring(start, finish - start).Split(':');
                                ud_make_a.Value = decimal.TryParse(split[0], out decimal a) ? decimal.Parse(split[0]) : 0;
                                ud_make_b.Value = decimal.TryParse(split[1], out decimal b) ? decimal.Parse(split[1]) : 0;
                            }
                            next += 1;
                        }
                        continue;
                    case 8:
                        find = l.IndexOf("model");
                        if (find > -1)
                        {
                            if (l.IndexOf("Unknown',") > -1) { tb_boat_model.Text = "None"; next += 1; continue; }
                            find = l.IndexOf("('") + 2;
                            tb_boat_model.Text = l.Substring(find, l.IndexOf("').") - find);
                            start = l.IndexOf("()[") + 3;
                            finish = l.IndexOf("] ");
                            if (start > 1 & finish > -1)
                            {
                                split = l.Substring(start, finish - start).Split(':');
                                ud_model_a.Value = decimal.TryParse(split[0], out decimal a) ? decimal.Parse(split[0]) : 0;
                                ud_model_b.Value = decimal.TryParse(split[1], out decimal b) ? decimal.Parse(split[1]) : 0;
                            }
                            next += 1;
                        }
                        continue;
                    case 9:
                        find = l.IndexOf("year");
                        if (find > -1)
                        {
                            if (l.IndexOf("0',") > -1) { tb_boat_year.Text = "None"; next += 1; continue; }
                            find = l.IndexOf("('") + 2;
                            tb_boat_year.Text = l.Substring(find, l.IndexOf("').") - find);
                            start = l.IndexOf("()[") + 3;
                            finish = l.IndexOf("] ");
                            if (start > 1 & finish > -1)
                            {
                                split = l.Substring(start, finish - start).Split(':');
                                ud_year_a.Value = decimal.TryParse(split[0], out decimal a) ? decimal.Parse(split[0]) : 0;
                                ud_year_b.Value = decimal.TryParse(split[1], out decimal b) ? decimal.Parse(split[1]) : 0;
                            }
                            next += 1;
                        }
                        continue;
                    case 10:
                        find = l.IndexOf("condition");
                        if (find > -1)
                        {
                            if (l.IndexOf("Unknown',") > -1) { tb_boat_condition.Text = "None"; next += 1; continue; }
                            find = l.IndexOf("('") + 2;
                            tb_boat_condition.Text = l.Substring(find, l.IndexOf("').") - find);
                            start = l.IndexOf("()[") + 3;
                            finish = l.IndexOf("] ");
                            if (start > 1 & finish > -1)
                            {
                                split = l.Substring(start, finish - start).Split(':');
                                ud_condition_a.Value = decimal.TryParse(split[0], out decimal a) ? decimal.Parse(split[0]) : 0;
                                ud_condition_b.Value = decimal.TryParse(split[1], out decimal b) ? decimal.Parse(split[1]) : 0;
                            }
                            next += 1;
                        }
                        continue;
                    case 11:
                        find = l.IndexOf("price");
                        if (find > -1)
                        {
                            if (l.IndexOf("0',") > -1) { tb_boat_price.Text = "None"; next += 1; continue; }
                            find = l.IndexOf("('") + 2;
                            tb_boat_price.Text = l.Substring(find, l.IndexOf("').") - find);
                            start = l.IndexOf("()[") + 3;
                            finish = l.IndexOf("] ");
                            if (start > 1 & finish > -1)
                            {
                                split = l.Substring(start, finish - start).Split(':');
                                ud_price_a.Value = decimal.TryParse(split[0], out decimal a) ? decimal.Parse(split[0]) : 0;
                                ud_price_b.Value = decimal.TryParse(split[1], out decimal b) ? decimal.Parse(split[1]) : 0;
                            }
                            next += 1;
                        }
                        continue;
                    case 12:
                        find = l.IndexOf("length");
                        if (find > -1)
                        {
                            if (l.IndexOf("0',") > -1) { tb_boat_length.Text = "None"; next += 1; continue; }
                            find = l.IndexOf("('") + 2;
                            tb_boat_length.Text = l.Substring(find, l.IndexOf("').") - find);
                            start = l.IndexOf("()[") + 3;
                            finish = l.IndexOf("] ");
                            if (start > 1 & finish > -1)
                            {
                                split = l.Substring(start, finish - start).Split(':');
                                ud_length_a.Value = decimal.TryParse(split[0], out decimal a) ? decimal.Parse(split[0]) : 0;
                                ud_length_b.Value = decimal.TryParse(split[1], out decimal b) ? decimal.Parse(split[1]) : 0;
                            }
                            next += 1;
                        }
                        continue;
                    case 13:
                        find = l.IndexOf("hull_material");
                        if (find > -1)
                        {
                            if (l.IndexOf("Unknown',") > -1) { tb_boat_material.Text = "None"; next += 1; continue; }
                            find = l.IndexOf("('") + 2;
                            tb_boat_material.Text = l.Substring(find, l.IndexOf("').") - find);
                            start = l.IndexOf("()[") + 3;
                            finish = l.IndexOf("] ");
                            if (start > 1 & finish > -1)
                            {
                                split = l.Substring(start, finish - start).Split(':');
                                ud_material_a.Value = decimal.TryParse(split[0], out decimal a) ? decimal.Parse(split[0]) : 0;
                                ud_material_b.Value = decimal.TryParse(split[1], out decimal b) ? decimal.Parse(split[1]) : 0;
                            }
                            next += 1;
                        }
                        continue;
                    case 14:
                        find = l.IndexOf("location");
                        if (find > -1)
                        {
                            if (l.IndexOf("Unknown',") > -1) { tb_boat_location.Text = "None"; next += 1; continue; }
                            find = l.IndexOf("('") + 2;
                            tb_boat_location.Text = l.Substring(find, l.IndexOf("').") - find);
                            start = l.IndexOf("()[") + 3;
                            finish = l.IndexOf("] ");
                            if (start > 1 & finish > -1)
                            {
                                split = l.Substring(start, finish - start).Split(':');
                                ud_location_a.Value = decimal.TryParse(split[0], out decimal a) ? decimal.Parse(split[0]) : 0;
                                ud_location_b.Value = decimal.TryParse(split[1], out decimal b) ? decimal.Parse(split[1]) : 0;
                            }
                            next += 1;
                        }
                        continue;
                    case 15:
                        find = l.IndexOf("country");
                        if (find > -1)
                        {
                            if (l.IndexOf("Unknown',") > -1) { tb_boat_country.Text = "None"; next += 1; continue; }
                            find = l.IndexOf("('") + 2;
                            tb_boat_country.Text = l.Substring(find, l.IndexOf("').") - find);
                            start = l.IndexOf("()[") + 3;
                            finish = l.IndexOf("] ");
                            if (start > 1 & finish > -1)
                            {
                                split = l.Substring(start, finish - start).Split(':');
                                ud_country_a.Value = decimal.TryParse(split[0], out decimal a) ? decimal.Parse(split[0]) : 0;
                                ud_country_b.Value = decimal.TryParse(split[1], out decimal b) ? decimal.Parse(split[1]) : 0;
                            }
                            next += 1;
                        }
                        continue;
                    case 16://post-processing
                        find = l.IndexOf('(');
                        if (find > -1)
                        {
                            if (l.IndexOf("string_contains_word") > -1)
                            {
                                if (l.IndexOf("make") > -1) { cb_sold_make.Checked = true; }
                                else if (l.IndexOf("model") > -1) { cb_sold_model.Checked = true; }
                                else if (l.IndexOf("year") > -1) { cb_sold_year.Checked = true; }
                                else if (l.IndexOf("condition") > -1) { cb_sold_condition.Checked = true; }
                                else if (l.IndexOf("price") > -1) { cb_sold_price.Checked = true; }
                                else if (l.IndexOf("length") > -1) { cb_sold_length.Checked = true; }
                                else if (l.IndexOf("hull_material") > -1) { cb_sold_material.Checked = true; }
                                else if (l.IndexOf("location") > -1) { cb_sold_location.Checked = true; }
                                else if (l.IndexOf("country") > -1) { cb_sold_country.Checked = true; }
                            }
                            if (l.IndexOf("parse_gbp") > -1)
                            {
                                cb_parse_gbp.Checked = true;
                                cb_startchar.Text = l.Substring(l.IndexOf(",'") + 2, 1);
                            }
                            else if (l.IndexOf("parse_feet") > -1)
                            {
                                cb_parse_feet.Checked = true;
                                if (l.IndexOf("'m'") > -1) { cb_convert_metres_to_feet.Checked = true; }
                            }
                        }
                        continue;
                    default:
                        break;
                }
            }
            btn_load_template.Text = "Clear Template!";
        }
        private void clear_template(bool includespider=true)
        {
            try { driver?.Close(); } catch { }
            tb_domain.Text = "";
            rtb_urls.Text = "";//tb_url.Text = "";
            tb_boat_listings.Text = "";
            tb_next_page.Text = "";
            tb_specifications.Text = "";
            tb_boat_make.Text = "None";
            tb_boat_model.Text = "None";
            tb_boat_year.Text = "None";
            tb_boat_condition.Text = "None";
            tb_boat_price.Text = "None";
            tb_boat_length.Text = "None";
            tb_boat_material.Text = "None";
            tb_boat_location.Text = "None";
            tb_boat_country.Text = "None";

            rtb_notes.Text = "";

            ud_make_a.Value = 0;
            ud_make_b.Value = 0;
            ud_model_a.Value = 0;
            ud_model_b.Value = 0;
            ud_year_a.Value = 0;
            ud_year_b.Value = 0;
            ud_condition_a.Value = 0;
            ud_condition_b.Value = 0;
            ud_price_a.Value = 0;
            ud_price_b.Value = 0;
            ud_length_a.Value = 0;
            ud_length_b.Value = 0;
            ud_material_a.Value = 0;
            ud_material_b.Value = 0;
            ud_location_a.Value = 0;
            ud_location_b.Value = 0;
            ud_country_a.Value = 0;
            ud_country_b.Value = 0;

            cb_status.SelectedIndex = 0;

            cb_sold_make.Checked = false;
            cb_sold_model.Checked = false;
            cb_sold_year.Checked = false;
            cb_sold_condition.Checked = false;
            cb_sold_price.Checked = false;
            cb_sold_length.Checked = false;
            cb_sold_material.Checked = false;
            cb_sold_location.Checked = false;
            cb_sold_country.Checked = false;

            cb_absolute_url.Checked = false;
            cb_infinite_scroll.Checked = false;
            cb_convert_metres_to_feet.Checked = false;
            cb_parse_feet.Checked = false;
            cb_parse_gbp.Checked = false;
            cb_startchar.Text = "£";

            if (includespider) 
            { 
                tb_spider_denomer.Text = "";
                btn_load_template.Text = "Load Template...";
                btn_load_template.Enabled = false;
            }
        }
        private void crawl_spider(char debug = 'k')//debug='c' when not debugging cmd
        {
            // get filename which is also the spider class name and spider name:
            string spider_name = tb_domain.Text.Split('.')[0];

            // grab root scrapy project directory
            string folder_path = Directory.GetCurrentDirectory();

            // clear csv first:
            try { if (File.Exists($@"{folder_path}\xpath_test.csv")) { File.Delete($@"{folder_path}\xpath_test.csv"); } }
            catch (IOException) { error_box("Please close xpath_test.csv before running this!"); return; }
            

            // prepare to send scrapy cmd line input to user's root scrapy project directory
            ProcessStartInfo procStart =
                new ProcessStartInfo("cmd", $"/{debug} scrapy crawl {spider_name} -o xpath_test.csv");
            procStart.WorkingDirectory = folder_path;
            Process proc = new Process();
            proc.StartInfo = procStart;

            // run spider from cmd and wait for it to close:
            proc.Start();
            proc.WaitForExit();

            // open the csv to user:
            proc.StartInfo.FileName = "xpath_test.csv";
            if (File.Exists($@"{folder_path}\xpath_test.csv")) { proc.Start(); }
            //proc.WaitForExit();
        }

        private void ll_xpath_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://devhints.io/xpath");
        }

        private void lb_spider_denomer_DoubleClick(object sender, EventArgs e)
        {
            success_box("The name of the spider file (excluding .py file extension), the name of the spider class (excluding 'Spider' suffix) and the name parameter of the spider class. Case insensitive.\n\n'example' input when Create Template! is clicked results in the creation of an ExampleSpider class with ExampleSpider.name = 'example' in a newly created example.py template spider file in the spiders subfolder.\n\nYour spider name can be anything, but should be the same as your website as a general rule so whoever looks at your spider file knows immediately what website it is scraping from.");
        }
        private void lb_domain_DoubleClick(object sender, EventArgs e)
        {
            success_box("The base domain of the website this spider is scraping. Scrapy needs this to determine which urls to scrape from, and more importantly, which urls not to scrap (as they are outside the domain scope). Case insensitive.\n\n'example.com' input results in ExampleSpider.allowed_domains = ['example.com'] in your spider.\n\nThis can technically handle multiple domains, however we have yet to discover any boat websites requiring this.If you run into a website with multiple domains, ask for advice.");
        }
        private void lb_url_DoubleClick(object sender, EventArgs e)
        {
            success_box("Every line of this Rich TextBox should contain a fully-formed URL. Scrapy needs these to determine which specific URL(s) to begin scraping from. Case insensitive.\n\n'http://www.example.com/speedboats-for-sale\nhttps://www.example.com/motorboats-for-sale\nhttp://www.example.com/houseboats-for-sale'\n\ninput results in ExampleSpider.start_urls =[f'http://www.example.com/speedboats-for-sale', f'http://www.example.com/motorboats-for-sale', f'http://www.example.com/houseboats-for-sale'] in your spider.\n\nThe[] means the result is a list. The f'' allows advanced f - string functionality in the URL(s) to allow for edge - cases like infinite scrolling websites.Ask for advice if you need to use this functionality.\n\nSome websites only require a single URL as all their boats are listed on a single boat listing page, like if all of example.com's boats were listed on 'https://www.example.com/boats-for-sale'");
        }
        private void lb_boat_listings_DoubleClick(object sender, EventArgs e)
        {
            success_box("The XPath that returns every single Boat Listing URL on a Boat Listing's webpage.\n\n'//example-tag/@href' could return 'http://www.example.com/a-boat','http://www.example.com/b-boat' and 'http://www.example.com/c-boat'.");
        }
        private void lb_next_page_DoubleClick(object sender, EventArgs e)
        {
            success_box("The XPath that returns the next page button's URL on a Boat Listing's webpage.\n\n'//example-tag(@class='next-page-button')/@href' could return 'http://www.example.com/boats-for-sale/page-2' then page 3 and so on until the button no longer exists on the last page of boat listings.");
        }
        private void lb_specifications_DoubleClick(object sender, EventArgs e)
        {
            success_box("The XPath that returns a Boat's specifications on the Boat's webpage. Optional as a website may occasionally not have a section for specs.\n\n'//example-tag[contains(text(),'specifications')]' could return the HTML for the boat's specifications - which the Single Boat Listing XPaths can then use to grab data from it.");
        }
        private void lb_boat_make_DoubleClick(object sender, EventArgs e)
        {
            success_box("The XPath that returns a Boat's specific Make from that Boat's webpage, if it is listed.\n\n'//example-tag/text()' could return the Make from the Boat's HTML. './ example - tag / text()' could return the Make from that Boat's Specifications(XPath) if within it.\n\nEvery boat needs either a Make or a Model XPath. Some websites will mix the pair into a single value which our pipelines will automatically split into Make and Model.\nIn this case, just put the Make+Model XPath into this Make XPath textbox, and our infrastructure will handle the rest.");
        }
        private void lb_boat_model_DoubleClick(object sender, EventArgs e)
        {
            success_box("The XPath that returns a Boat's specific Model from that Boat's webpage, if it is listed.\n\n'//example-tag/text()' could return the Model from that Boat's HTML. './ example - tag / text()' could return the Model from that Boat's Specifications(XPath) if within it.\n\nEvery boat needs either a Make or a Model XPath. Some websites will mix the pair into a single value which our pipelines will automatically split into Make and Model.\nIn this case, just put the Make+Model XPath into the Make XPath textbox above, and our infrastructure will handle the rest.");
        }
        private void lb_boat_year_DoubleClick(object sender, EventArgs e)
        {
            success_box("The XPath that returns a Boat's specific Year from that Boat's webpage, if it is listed.\n\n'//example-tag/text()' could return the Year from that Boat's HTML. './ example - tag / text()' could return the Year from that Boat's Specifications(XPath).\n\nYear must end up formatted as a strictly - numerical format.\nExample: '2004' is fine but '01/01/2004' would require the first 6 characters to be removed using the numerical dials to the right of the textbox(6 in first box)");
        }
        private void lb_boat_condition_DoubleClick(object sender, EventArgs e)
        {
            success_box("The XPath that returns the 'Condition' of a Boat from that Boat's webpage, if it is listed.\n\n'//example-tag/text()' could return the Condition from that Boat's HTML. './ example - tag / text()' could return the Condition from that Boat's Specifications(XPath).\n\nTypically a condition is either Used or New, and will be processed in the pipeline as such so you don't have to worry about edge cases here.");
        }
        private void lb_boat_price_DoubleClick(object sender, EventArgs e)
        {
            success_box("The XPath that returns the Price of a Boat from that Boat's webpage, if it is listed.\nThis is ideally in GBP, but can handle American Dollars, French Euros and Japanese Yen.\nIf you must add a currency to the list, ask for assistance.\n\n'//example-tag/text()' could return the Price from that Boat's HTML. './ example - tag / text()' could return the Price from that Boat's Specifications(XPath).\n\nPrice must end up formatted as a strictly - numerical format.\n  Example: '100,000.00' is fine, but '£100,000.00' would require the £ sign removed. This is done by checking 'Parse Price as GBP' below.\n  Example: '100,000.00' is fine, but '$100,000.00' would require the $ sign removed. This is done by checking 'Parse Price as GBP' below,\nthen changing the combobox to the right of it to a $ sign rather than the default £ sign.");
        }
        private void lb_boat_length_DoubleClick(object sender, EventArgs e)
        {
            success_box("The XPath that returns the Length of a Boat from that Boat's webpage, if it is listed.\nThis is ideally in Imperial Feet(ft), but can handle Metres as well.\nIf you must add a unit of measurement to the list, ask for assistance.\n\n'//example-tag/text()' could return the Length from that Boat's HTML. './ example - tag / text()' could return the Length from that Boat's Specifications(XPath).\n\nLength must end up formatted as a strictly - numerical format.\n  Example: '123' is fine, but '12'3\"ft' would require all the symbols removed. This is done by checking 'Parse Length as Feet' below.\n  Example: '123' is fine, but '123 metres' would require the ' metres' symbol removed. This is done by checking 'Parse Length as Feet' below, \nthen checking 'Convert Metres to Feet' to the right of it.");
        }
        private void lb_boat_material_DoubleClick(object sender, EventArgs e)
        {
            success_box("The XPath that returns the Hull Material of a Boat from that Boat's webpage, if it is listed.\n\n'//example-tag/text()' could return the Material from that Boat's HTML. './ example - tag / text()' could return the Material from that Boat's Specifications(XPath).\n\nWe will narrow down the results in the pipeline so you don't need to worry about weird edge-cases here.");
        }
        private void lb_boat_location_DoubleClick(object sender, EventArgs e)
        {
            success_box("The XPath that returns a Boat's current docking/mooring Location from that Boat's webpage, if it is listed.\n\n'//example-tag/text()' could return the Location from the Boat's HTML. './ example - tag / text()' could return the Location from that Boat's Specifications(XPath) if within it.\n\nEvery boat needs either a Country or Location XPath.Some websites will mix the pair into a single value which our pipelines will automatically split into Country and Location.\nIn this case, just put the Country+Location XPath into this Location XPath textbox, and our infrastructure will handle the rest.");
        }
        private void lb_boat_country_DoubleClick(object sender, EventArgs e)
        {
            success_box("The XPath that returns a Boat's current docking/mooring Country from that Boat's webpage, if it is listed.\n\n'//example-tag/text()' could return the Country from the Boat's HTML. './ example - tag / text()' could return the Country from that Boat's Specifications(XPath) if within it.\n\nEvery boat needs either a Country or Location XPath.Some websites will mix the pair into a single value which our pipelines will automatically split into Country and Location.\nIn this case, just put the Country+Location XPath into the Location XPath textbox, and our infrastructure will handle the rest. ");
        }

        private void btn_selenium_Click(object sender, EventArgs e)
        {
            declare_selenium();
            driver.Navigate().GoToUrl(rtb_urls.Lines[0]);
        }
        private void declare_selenium()
        {
            driver = new ChromeDriver();
            jsExecutor = (IJavaScriptExecutor)driver;
        }
    }
}

// Boat Data
// future Boat Sellers