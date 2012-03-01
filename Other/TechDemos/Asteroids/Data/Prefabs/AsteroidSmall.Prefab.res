﻿<?xml version="1.0" encoding="utf-8"?>
<root>
  <object dataType="Class" type="Duality.Resources.Prefab" id="1">
    <objTree dataType="Class" type="Duality.GameObject" id="2">
      <prefabLink />
      <parent />
      <children />
      <compMap dataType="Class" type="System.Collections.Generic.Dictionary`2[[System.Type],[Duality.Component]]" id="3" surrogate="true">
        <customSerialIO />
        <customSerialIO>
          <keys dataType="Array" type="System.Type[]" id="4" length="4">
            <object dataType="Type" id="5" value="Duality.Components.Transform" />
            <object dataType="Type" id="6" value="Duality.Components.Renderers.AnimSpriteRenderer" />
            <object dataType="Type" id="7" value="GamePlugin.Asteroid" />
            <object dataType="Type" id="8" value="Duality.Components.Collider" />
          </keys>
          <values dataType="Array" type="Duality.Component[]" id="9" length="4">
            <object dataType="Class" type="Duality.Components.Transform" id="10">
              <pos dataType="Struct" type="OpenTK.Vector3">
                <X dataType="Float">0</X>
                <Y dataType="Float">0</Y>
                <Z dataType="Float">0</Z>
              </pos>
              <vel dataType="Struct" type="OpenTK.Vector3">
                <X dataType="Float">0.5256745</X>
                <Y dataType="Float">0.114168033</Y>
                <Z dataType="Float">0</Z>
              </vel>
              <angle dataType="Float">3.07849479</angle>
              <angleVel dataType="Float">-0.009994157</angleVel>
              <scale dataType="Struct" type="OpenTK.Vector3">
                <X dataType="Float">1</X>
                <Y dataType="Float">1</Y>
                <Z dataType="Float">1</Z>
              </scale>
              <deriveAngle dataType="Bool">true</deriveAngle>
              <extUpdater />
              <changes dataType="Enum" type="Duality.Components.Transform+DirtyFlags" name="None" value="0" />
              <parentTransform />
              <posAbs dataType="Struct" type="OpenTK.Vector3">
                <X dataType="Float">0</X>
                <Y dataType="Float">0</Y>
                <Z dataType="Float">0</Z>
              </posAbs>
              <velAbs dataType="Struct" type="OpenTK.Vector3">
                <X dataType="Float">0.5256745</X>
                <Y dataType="Float">0.114168033</Y>
                <Z dataType="Float">0</Z>
              </velAbs>
              <angleAbs dataType="Float">3.07849479</angleAbs>
              <angleVelAbs dataType="Float">-0.009994157</angleVelAbs>
              <scaleAbs dataType="Struct" type="OpenTK.Vector3">
                <X dataType="Float">1</X>
                <Y dataType="Float">1</Y>
                <Z dataType="Float">1</Z>
              </scaleAbs>
              <gameobj dataType="ObjectRef">2</gameobj>
              <disposed dataType="Bool">false</disposed>
              <active dataType="Bool">true</active>
            </object>
            <object dataType="Class" type="Duality.Components.Renderers.AnimSpriteRenderer" id="11">
              <animFirstFrame dataType="Int">0</animFirstFrame>
              <animFrameCount dataType="Int">2</animFrameCount>
              <animDuration dataType="Float">1</animDuration>
              <animLoopMode dataType="Enum" type="Duality.Components.Renderers.AnimSpriteRenderer+LoopMode" name="RandomSingle" value="3" />
              <animTime dataType="Float">0.890709341</animTime>
              <animCycle dataType="Int">0</animCycle>
              <verticesSmooth />
              <rect dataType="Struct" type="Duality.Rect">
                <x dataType="Float">-18</x>
                <y dataType="Float">-20</y>
                <w dataType="Float">36</w>
                <h dataType="Float">39</h>
              </rect>
              <sharedMat dataType="Struct" type="Duality.ContentRef`1[[Duality.Resources.Material]]">
                <contentPath dataType="String">Data\Materials\SmallAsteroid.Material.res</contentPath>
              </sharedMat>
              <customMat />
              <colorTint dataType="Struct" type="Duality.ColorFormat.ColorRgba">
                <r dataType="Byte">255</r>
                <g dataType="Byte">255</g>
                <b dataType="Byte">255</b>
                <a dataType="Byte">255</a>
              </colorTint>
              <rectMode dataType="Enum" type="Duality.Components.Renderers.SpriteRenderer+UVMode" name="Stretch" value="0" />
              <renderFlags dataType="Enum" type="Duality.RendererFlags" name="Default" value="3" />
              <visibilityGroup dataType="UInt">1</visibilityGroup>
              <gameobj dataType="ObjectRef">2</gameobj>
              <disposed dataType="Bool">false</disposed>
              <active dataType="Bool">true</active>
            </object>
            <object dataType="Class" type="GamePlugin.Asteroid" id="12">
              <hp dataType="Float">50</hp>
              <type dataType="Enum" type="GamePlugin.AsteroidType" name="Small" value="0" />
              <powerup dataType="Enum" type="GamePlugin.PowerupType" name="None" value="0" />
              <gameobj dataType="ObjectRef">2</gameobj>
              <disposed dataType="Bool">false</disposed>
              <active dataType="Bool">true</active>
            </object>
            <object dataType="Class" type="Duality.Components.Collider" id="13">
              <bodyType dataType="Enum" type="Duality.Components.Collider+BodyType" name="Dynamic" value="1" />
              <linearDamp dataType="Float">0</linearDamp>
              <angularDamp dataType="Float">0</angularDamp>
              <fixedAngle dataType="Bool">false</fixedAngle>
              <ignoreGravity dataType="Bool">false</ignoreGravity>
              <colCat dataType="Enum" type="FarseerPhysics.Dynamics.Category" name="Cat1" value="1" />
              <colWith dataType="Enum" type="FarseerPhysics.Dynamics.Category" name="All" value="2147483647" />
              <shapes dataType="Class" type="System.Collections.Generic.List`1[[Duality.Components.Collider+ShapeInfo]]" id="14">
                <_items dataType="Array" type="Duality.Components.Collider+ShapeInfo[]" id="15" length="4">
                  <object dataType="Class" type="Duality.Components.Collider+CircleShapeInfo" id="16">
                    <radius dataType="Float">17.0660477</radius>
                    <position dataType="Struct" type="OpenTK.Vector2">
                      <X dataType="Float">-0</X>
                      <Y dataType="Float">0</Y>
                    </position>
                    <parent dataType="ObjectRef">13</parent>
                    <density dataType="Float">1</density>
                    <friction dataType="Float">0.3</friction>
                    <restitution dataType="Float">1</restitution>
                    <sensor dataType="Bool">false</sensor>
                  </object>
                  <object />
                  <object />
                  <object />
                </_items>
                <_size dataType="Int">1</_size>
                <_version dataType="Int">1</_version>
              </shapes>
              <gameobj dataType="ObjectRef">2</gameobj>
              <disposed dataType="Bool">false</disposed>
              <active dataType="Bool">true</active>
            </object>
          </values>
        </customSerialIO>
      </compMap>
      <compList dataType="Class" type="System.Collections.Generic.List`1[[Duality.Component]]" id="17">
        <_items dataType="Array" type="Duality.Component[]" id="18" length="4">
          <object dataType="ObjectRef">10</object>
          <object dataType="ObjectRef">11</object>
          <object dataType="ObjectRef">12</object>
          <object dataType="ObjectRef">13</object>
        </_items>
        <_size dataType="Int">4</_size>
        <_version dataType="Int">4</_version>
      </compList>
      <name dataType="String">AsteroidSmall</name>
      <active dataType="Bool">true</active>
      <disposed dataType="Bool">false</disposed>
      <compTransform dataType="ObjectRef">10</compTransform>
      <EventComponentAdded />
      <EventComponentRemoving />
      <EventCollisionBegin />
      <EventCollisionEnd />
      <EventCollisionSolve />
    </objTree>
  </object>
</root>