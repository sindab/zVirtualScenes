﻿Ext.require(['Ext.Panel', 'Ext.util.JSONP', 'Ext.MessageBox'], function () {
    Ext.define('zvsMobile.view.GroupDetails', {
        extend: 'Ext.Panel',
        xtype: 'GroupDetails',
        constructor: function (config) {
            var self = this;
            self.RepollTimer;
            Ext.apply(config || {}, {                
                layout: {
                    type: 'vbox',
                    align: 'strech'
                },
                items: [{
                    xtype: 'toolbar',
                    docked: 'top',
                    title: 'Group Details',
                    items: [{
                        xtype: 'button',
                        iconMask: true,
                        ui: 'back',
                        text: 'Back',
                        handler: function () {
                            var ViewPort = self.parent;
                            ViewPort.getLayout().setAnimation({ type: 'slide', direction: 'right' });
                            ViewPort.setActiveItem(ViewPort.items.items[0]);
                        }
                    }]
                }, {
                    xtype: 'panel',
                    tpl: new Ext.XTemplate(
                        '<div class="group_info">',
                            '<div class="head">',
							    '<div class="image"></div>',
							    '<tpl for="group">',
					                '<h1>{name}</h1>',
                                    '</tpl>',
                             '</div>',
                        '</div>')
                }, {
                    xtype: 'button',
                    text: 'Turn On',
                    ui: 'confirm',
                    margin: '25 5 5 5',
                    handler: function () {
                        console.log('AJAX: ActivateGroup' + self.groupId);
                        Ext.Ajax.request({
                            url: '/API/commands/',
                            method: 'POST',
                            params: {
                                u: Math.random(),
                                name: 'GROUP_ON',
                                arg: self.groupId
                            },
                            success: function (response, opts) {
                                var result = JSON.parse(response.responseText);
                                if (result.success) {
                                    Ext.Msg.alert('Group', "All device in group Turned On");
                                }
                                else {
                                    Ext.Msg.alert('Group', 'Communication Error!');
                                }
                            }
                        }); 
                    }
                }, {
                    xtype: 'button',
                    text: 'Turn Off',
                    ui: 'confirm',
                    margin: '5 5 25 5',
                    handler: function () {
                        console.log('AJAX: DeactivateGroup' + self.groupId);
                        Ext.Ajax.request({
                            url: '/API/commands/',
                            method: 'POST',
                            params: {
                                u: Math.random(),
                                name: 'GROUP_OFF',
                                arg: self.groupId
                            },
                            success: function (response, opts) {
                                var result = JSON.parse(response.responseText);
                                if (result.success) {
                                    Ext.Msg.alert('Group', "All device in group Turned Off");
                                }
                                else {
                                    Ext.Msg.alert('Group', 'Communication Error!');
                                }
                            }
                        });                       

                    }
                }, {
                    xtype: 'panel',
                    tpl: new Ext.XTemplate(
                     	'<tpl for="group">',
                         //'<tpl if="devices.length &gt; 0">',
                            '<div class="group_overview">',
                                '<table class="info">',
                                '<thead>',
                                    '<tr>',
                                        '<th scope="col" abbr="Device">Device</th>',
                                        '<th scope="col" abbr="Action">Type</th>',
                                    '</tr>',
                                '</thead>',
                                '<tbody>',
                                '<tpl for="devices">',
                                        '<tr>',
                                            '<td>{name}</td>',
                                            '<td>{type}</td>',
                                            '</tr>',
                                        '</tpl>',
                                '</tbody>',
                                '</table>',
                            '</div>',
                        //'</tpl>',
                        '</tpl>'
                    )

                }]
            });
            this.callOverridden([config]);
        },
        config: {
	    layout: 'fit',
	    scrollable: 'vertical'
	},
    delayedReload: function () {
                    var self = this; 
                    if (self.RepollTimer) { 
                    clearInterval(self.RepollTimer); 
                    }

                    self.RepollTimer = setTimeout(function () {
                        self.loadGroup(self.groupId);
                    }, 500);
                }, 
                    loadGroup: function (groupID) {
                    var self = this; 
                    self.groupId = groupID;
                    //Get Device Details			
                    console.log('AJAX: GetgroupDetails');
                    
                    Ext.data.JsonP.request({
                        url: '/API/group/' + self.groupId,
                        callbackKey: 'callback',
                        params: {
                            u: Math.random()
                        },
                        success: function (result) {
                            console.log(self.items.items[1]);
                            //Send data to panel TPL                            
                            self.items.items[1].setData(result);
                            self.items.items[4].setData(result);
                        }
                    });  
                   
                },
    });
});